using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using UKPS.Api.WebApi.Authorisation;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Controllers;

internal sealed class ProducesAuthorisationResponsesConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                if (
                    controller.Attributes.OfType<IAllowAnonymous>().Any()
                    || action.Attributes.OfType<IAllowAnonymous>().Any()
                )
                {
                    continue;
                }

                IAuthorizeData[] authorizeData = controller
                    .Attributes.Concat(action.Attributes)
                    .OfType<IAuthorizeData>()
                    .ToArray();
                if (authorizeData.Length == 0)
                {
                    continue;
                }

                AddProducesResponse(action, StatusCodes.Status401Unauthorized);

                // Endpoints using the organisation selection policy do not act on the selected
                // organisation, so cannot conflict with it.
                if (
                    !authorizeData.Any(x =>
                        string.Equals(
                            x.Policy,
                            AuthorisationPolicies.OrganisationSelection,
                            StringComparison.Ordinal
                        )
                    )
                )
                {
                    // Only specific typed contracts replace the authentication schema; generic
                    // ProblemDetails does not describe its code or organisationId members.
                    // Specific domain contracts must include selection conflicts themselves.
                    bool hasConflictContract = controller
                        .Attributes.Concat(action.Attributes)
                        .Concat(controller.Filters)
                        .Concat(action.Filters)
                        .OfType<IApiResponseMetadataProvider>()
                        .Any(x =>
                            x.StatusCode == StatusCodes.Status409Conflict
                            && x.Type is not null
                            && x.Type != typeof(void)
                            && x.Type != typeof(ProblemDetails)
                        );
                    if (!hasConflictContract)
                    {
                        AddProducesResponse(action, StatusCodes.Status409Conflict);
                    }
                }
            }
        }
    }

    private static void AddProducesResponse(ActionModel action, int statusCode) =>
        action.Filters.Add(
            new ProducesResponseTypeAttribute(typeof(AuthenticationProblemDetails), statusCode)
        );
}
