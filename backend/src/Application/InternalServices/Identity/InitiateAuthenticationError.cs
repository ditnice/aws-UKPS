using UKPS.Api.Application.Authentication.Errors;

namespace UKPS.Api.Application.InternalServices.Identity;

internal abstract record InitiateAuthenticationError
{
    public sealed record Unauthorised : InitiateAuthenticationError;

    public sealed record Challenge(UkpsChallengeType ChallengeType, string AuthenticationSessionId)
        : InitiateAuthenticationError;
}
