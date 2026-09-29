using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Controllers;

internal sealed class ProducesUnauthorizedResponseConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                var allowsAnonymous =
                    controller.Attributes.OfType<IAllowAnonymous>().Any()
                    || action.Attributes.OfType<IAllowAnonymous>().Any();
                var requiresAuthorization =
                    controller.Attributes.OfType<IAuthorizeData>().Any()
                    || action.Attributes.OfType<IAuthorizeData>().Any();

                if (allowsAnonymous || !requiresAuthorization)
                {
                    continue;
                }

                action.Filters.Add(
                    new ProducesResponseTypeAttribute(
                        typeof(AuthenticationProblemDetails),
                        StatusCodes.Status401Unauthorized
                    )
                );
            }
        }
    }
}
