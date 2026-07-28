using System.Diagnostics.CodeAnalysis;

namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents an error that can occur during the login process.
/// </summary>
public abstract record LoginError
{
    /// <summary>
    /// Prevents direct instantiation of login errors.
    /// </summary>
    protected LoginError() { }

    /// <summary>
    /// Represents an authentication challenge that must be completed before authentication can succeed.
    /// </summary>
    public sealed record Challenge : LoginError
    {
        /// <summary>
        /// Gets the authentication challenge that must be completed.
        /// </summary>
        public required UkpsChallengeType Type { get; init; }

        /// <summary>
        /// Gets the authentication session identifier required to complete the challenge.
        /// </summary>
        public required string AuthenticationSessionId { get; init; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Challenge"/> record.
        /// </summary>
        /// <param name="type">
        /// The authentication challenge that must be completed.
        /// </param>
        /// <param name="authenticationSessionId">
        /// The authentication session identifier required to complete the challenge.
        /// </param>
        [SetsRequiredMembers]
        public Challenge(UkpsChallengeType type, string authenticationSessionId)
        {
            Type = type;
            AuthenticationSessionId = authenticationSessionId;
        }
    }

    /// <summary>
    /// Represents an error indicating that the supplied credentials were not authorised.
    /// </summary>
    public sealed record Unauthorised : LoginError;
}
