using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Shouldly;
using UKPS.Api.WebApi.Authorisation;
using UKPS.Api.WebApi.Controllers;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class ProducesAuthorisationResponsesConventionTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Apply_GenericOrUntypedConflict_KeepsAuthenticationFallback(
        bool onController,
        bool asFilter,
        bool untyped
    )
    {
        var contract = untyped
            ? new ProducesResponseTypeAttribute(409)
            : new ProducesResponseTypeAttribute(typeof(ProblemDetails), 409);
        object[] controllerAttributes = [new AuthorizeAttribute()];
        object[] actionAttributes = [];
        if (!asFilter)
        {
            if (onController)
            {
                controllerAttributes = [.. controllerAttributes, contract];
            }
            else
            {
                actionAttributes = [contract];
            }
        }

        var (application, controller, action) = Model(controllerAttributes, actionAttributes);
        if (asFilter)
        {
            if (onController)
            {
                controller.Filters.Add(contract);
            }
            else
            {
                action.Filters.Add(contract);
            }
        }

        new ProducesAuthorisationResponsesConvention().Apply(application);

        var responses = action.Filters.OfType<IApiResponseMetadataProvider>().ToArray();
        responses.ShouldContain(x =>
            x.StatusCode == 409 && x.Type == typeof(AuthenticationProblemDetails)
        );
        responses.ShouldContain(x =>
            x.StatusCode == 401 && x.Type == typeof(AuthenticationProblemDetails)
        );
    }

    [Fact]
    public void Apply_SubmissionDeclaredContract_DescribesDomainAndSelectionConflicts()
    {
        var (application, _, action) = Model(
            typeof(RecordSubmissionController).GetCustomAttributes(inherit: true),
            typeof(RecordSubmissionController)
                .GetMethod(nameof(RecordSubmissionController.SubmitRecord))!
                .GetCustomAttributes(inherit: true)
        );

        new ProducesAuthorisationResponsesConvention().Apply(application);

        var contracts = action
            .Attributes.Concat(action.Filters)
            .OfType<IApiResponseMetadataProvider>()
            .Where(x => x.StatusCode == 409)
            .ToArray();
        contracts.ShouldHaveSingleItem().Type.ShouldBe(typeof(SubmissionConflictProblemDetails));
        typeof(SubmissionConflictProblemDetails)
            .GetProperty(nameof(SubmissionConflictProblemDetails.Code))!
            .PropertyType.ShouldBe(typeof(string));
        typeof(SubmissionConflictProblemDetails)
            .GetProperty(nameof(SubmissionConflictProblemDetails.OrganisationId))!
            .PropertyType.ShouldBe(typeof(int?));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Apply_ExplicitTypedConflict_IsPreserved(bool onController, bool asFilter)
    {
        var contract = new ProducesResponseTypeAttribute(
            typeof(SubmissionConflictProblemDetails),
            409
        );
        object[] controllerAttributes = [new AuthorizeAttribute()];
        object[] actionAttributes = [];
        if (!asFilter)
        {
            if (onController)
            {
                controllerAttributes = [.. controllerAttributes, contract];
            }
            else
            {
                actionAttributes = [contract];
            }
        }

        var (application, controller, action) = Model(controllerAttributes, actionAttributes);
        if (asFilter)
        {
            if (onController)
            {
                controller.Filters.Add(contract);
            }
            else
            {
                action.Filters.Add(contract);
            }
        }

        new ProducesAuthorisationResponsesConvention().Apply(application);

        action
            .Filters.OfType<IApiResponseMetadataProvider>()
            .ShouldNotContain(x =>
                x.StatusCode == 409 && x.Type == typeof(AuthenticationProblemDetails)
            );
        action
            .Filters.OfType<IApiResponseMetadataProvider>()
            .ShouldContain(x =>
                x.StatusCode == 401 && x.Type == typeof(AuthenticationProblemDetails)
            );
    }

    [Theory]
    [InlineData("authorised", true, true)]
    [InlineData("selection", true, false)]
    [InlineData("anonymousAction", false, false)]
    [InlineData("anonymousController", false, false)]
    [InlineData("unprotected", false, false)]
    public void Apply_AuthenticationMetadata_RespectsPolicies(
        string scenario,
        bool hasUnauthorised,
        bool hasConflict
    )
    {
        object[] attributes = scenario switch
        {
            "unprotected" => [],
            "selection" => [new AuthorizeAttribute(AuthorisationPolicies.OrganisationSelection)],
            "anonymousController" => [new AuthorizeAttribute(), new AllowAnonymousAttribute()],
            _ => [new AuthorizeAttribute()],
        };
        var (application, _, action) = Model(
            attributes,
            string.Equals(scenario, "anonymousAction", StringComparison.Ordinal)
                ? [new AllowAnonymousAttribute()]
                : []
        );

        new ProducesAuthorisationResponsesConvention().Apply(application);

        var responses = action.Filters.OfType<IApiResponseMetadataProvider>().ToArray();
        responses.Any(x => x.StatusCode == 401).ShouldBe(hasUnauthorised);
        responses.Any(x => x.StatusCode == 409).ShouldBe(hasConflict);
        responses.ShouldAllBe(x => x.Type == typeof(AuthenticationProblemDetails));
    }

    private static (ApplicationModel, ControllerModel, ActionModel) Model(
        object[] controllerAttributes,
        object[] actionAttributes
    )
    {
        var application = new ApplicationModel();
        var controller = new ControllerModel(
            typeof(RecordSubmissionController).GetTypeInfo(),
            controllerAttributes
        );
        var action = new ActionModel(
            typeof(RecordSubmissionController).GetMethod(
                nameof(RecordSubmissionController.SubmitRecord)
            )!,
            actionAttributes
        )
        {
            Controller = controller,
        };
        controller.Actions.Add(action);
        application.Controllers.Add(controller);
        return (application, controller, action);
    }
}
