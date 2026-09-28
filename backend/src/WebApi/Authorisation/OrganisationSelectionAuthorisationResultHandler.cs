using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using UKPS.Api.WebApi.CustomResponses;
using UKPS.Api.WebApi.InternalServices.Identity;

namespace UKPS.Api.WebApi.Authorisation;

/// <summary>
/// Returns an unauthorised response containing the relevant <see cref="AuthenticationFailCode"/>
/// when a user who has not yet selected an organisation attempts to access an endpoint that
/// requires one, so that clients can direct the user to select an organisation.
/// </summary>
internal sealed class OrganisationSelectionAuthorisationResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult
    )
    {
        string? selectionFailure = context
            .User.FindFirst(UkpsClaimTypes.OrganisationSelectionFailure)
            ?.Value;

        if (
            (authorizeResult.Forbidden || authorizeResult.Challenged)
            && Enum.TryParse(selectionFailure, out AuthenticationFailCode code)
        )
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(
                new AuthenticationProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Code = code,
                },
                context.RequestAborted
            );
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
