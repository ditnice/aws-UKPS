using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Application.Authentication.Errors;

namespace UKPS.Api.WebApi.CustomResponses;

/// <summary>
/// Represents problem details returned when authentication fails or
/// additional authentication is required.
/// </summary>
public sealed class AuthenticationProblemDetails : ProblemDetails
{
    /// <summary>
    /// Gets the type of authentication challenge required to complete authentication.
    /// </summary>
    public UkpsChallengeType? ChallengeType { get; init; }

    /// <summary>
    /// Gets the session identifier associated with the authentication challenge.
    /// </summary>
    public string? AuthenticationSession { get; init; }

    /// <summary>
    /// Creates an authentication problem details response indicating that
    /// authentication was unsuccessful.
    /// </summary>
    /// <returns>
    /// An <see cref="AuthenticationProblemDetails"/> representing an unauthorised response.
    /// </returns>
    public static AuthenticationProblemDetails Unauthorised() =>
        new()
        {
            Title = "Unauthorised",
            Detail = "Invalid credentials or insufficient permissions.",
        };

    /// <summary>
    /// Creates an authentication problem details response indicating that
    /// an additional authentication challenge is required.
    /// </summary>
    /// <param name="challengeType">
    /// The type of authentication challenge that must be completed.
    /// </param>
    /// <param name="authenticationSession">
    /// The session identifier required to continue the authentication flow.
    /// </param>
    /// <returns>
    /// An <see cref="AuthenticationProblemDetails"/> containing the required
    /// authentication challenge information.
    /// </returns>
    public static AuthenticationProblemDetails Challenge(
        UkpsChallengeType challengeType,
        string authenticationSession
    ) =>
        new()
        {
            Title = "Unauthorised",
            Detail = "Additional authentication required.",
            ChallengeType = challengeType,
            AuthenticationSession = authenticationSession,
        };
}
