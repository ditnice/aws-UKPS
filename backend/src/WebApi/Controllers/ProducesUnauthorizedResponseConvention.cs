using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
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
                if (action.Filters.Any(f => f is IAllowAnonymousFilter))
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
