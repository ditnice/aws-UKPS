using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                    AddProducesResponse(action, StatusCodes.Status409Conflict);
                }
            }
        }
    }

    private static void AddProducesResponse(ActionModel action, int statusCode) =>
        action.Filters.Add(
            new ProducesResponseTypeAttribute(typeof(AuthenticationProblemDetails), statusCode)
        );
}
