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
    /// Indicates that authentication cannot be completed until the user's identity
    /// has been verified.
    /// </summary>
    public sealed record VerificationRequired : InitiateAuthenticationError;

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
        public required string AuthenticationSession { get; init; }

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
            AuthenticationSession = authenticationSessionId;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Challenge"/> record.
        /// </summary>
        public Challenge() { }
    }

    /// <summary>
    /// Matches the error against its possible types and invokes the corresponding handler.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the handlers.</typeparam>
    /// <param name="unauthorised">
    /// The handler invoked when authentication is unauthorised.
    /// </param>
    /// <param name="verificationRequired">
    /// The handler invoked when identity verification is required.
    /// </param>
    /// <param name="challenge">
    /// The handler invoked when an authentication challenge must be completed.
    /// </param>
    /// <returns>The result returned by the matching handler.</returns>
    internal TResult Match<TResult>(
        Func<TResult> unauthorised,
        Func<TResult> verificationRequired,
        Func<Challenge, TResult> challenge
    ) =>
        this switch
        {
            Unauthorised => unauthorised(),
            VerificationRequired => verificationRequired(),
            Challenge c => challenge(c),
            _ => throw new UnreachableException(),
        };
}
