using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Authentication.Dtos
{
    /// <summary>
    /// Represents the details required to verify a user's multi-factor authentication setup.
    /// </summary>
    public record VerifyMultiFactorAuthenticationCommand
        : VerifyMultiFactorAuthenticationUpdateCommand
    {
        /// <summary>
        /// Gets the unique token associated with the user onboarding record.
        /// </summary>
        [Required]
        public required Guid SetupToken { get; init; }

        /// <summary>
        /// Gets the authentication session identifier required to complete the
        /// multi-factor authentication verification flow.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public required string AuthenticationSession { get; init; }
    }
}
