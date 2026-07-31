using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UKPS.Api.Application.Authentication.Errors;

namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents an error or additional action required when initiating authentication.
/// </summary>
public abstract record InitiateAuthenticationError
{
    /// <summary>
    /// Indicates that the authentication request could not be authorised.
    /// </summary>
    public sealed record Unauthorised : InitiateAuthenticationError;

    /// <summary>
    /// Indicates that authentication cannot be completed until the specified challenge
    /// has been satisfied.
    /// </summary>
    public sealed record Challenge : InitiateAuthenticationError
    {
        /// <summary>
        /// Gets the type of authentication challenge that must be completed.
        /// </summary>
        public required UkpsChallengeType ChallengeType { get; init; }

        /// <summary>
        /// Gets the authentication session identifier that must be supplied when responding
        /// to the challenge.
        /// </summary>
        public required string AuthenticationSessionId { get; init; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Challenge"/> record.
        /// </summary>
        /// <param name="challengeType">
        /// The type of authentication challenge that must be completed.
        /// </param>
        /// <param name="authenticationSessionId">
        /// The authentication session identifier that must be supplied when responding
        /// to the challenge.
        /// </param>
        [SetsRequiredMembers]
        public Challenge(UkpsChallengeType challengeType, string authenticationSessionId)
        {
            ChallengeType = challengeType;
            AuthenticationSessionId = authenticationSessionId;
        }
    }

    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<Challenge, TResult> challenge
    ) =>
        this switch
        {
            Unauthorised => unauthorised(),
            Challenge c => challenge(c),
            _ => throw new UnreachableException(),
        };
}
