using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace UKPS.Api.WebApi;

/// <summary>
/// Handles additional processing after a bearer token has been successfully validated.
/// </summary>
public interface ITokenValidationHandler
{
    /// <summary>
    /// Handles a successfully validated token.
    /// </summary>
    /// <param name="context">
    /// The context containing information about the validated token and the current HTTP request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous handling operation.
    /// </returns>
    Task Handle(TokenValidatedContext context, CancellationToken cancellationToken);
}
