using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.WebApi.CustomResponses;

namespace UKPS.Api.WebApi.Authorisation;

/// <summary>
/// Returns a conflict response containing <see cref="AuthenticationFailCode.SelectedOrganisationMismatch"/>
/// when the user requests an organisation they belong to that is not their selected organisation,
/// so that clients can direct the user to select it.
/// </summary>
internal sealed class SelectedOrganisationMismatchExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not SelectedOrganisationMismatchException exception)
        {
            return;
        }

        context.Result = new ObjectResult(
            new AuthenticationProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "The requested organisation is not your selected organisation.",
                Code = AuthenticationFailCode.SelectedOrganisationMismatch,
                OrganisationId = exception.OrganisationId,
            }
        )
        {
            StatusCode = StatusCodes.Status409Conflict,
        };
        context.ExceptionHandled = true;
    }
}
