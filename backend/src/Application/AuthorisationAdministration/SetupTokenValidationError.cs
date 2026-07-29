namespace UKPS.Api.Application.AuthorisationAdministration;

/// <summary>
/// Represents an error that can occur when validating a setup token.
/// </summary>
public abstract record SetupTokenValidationError
{
    /// <summary>
    /// Indicates that the setup token has expired and is no longer valid.
    /// </summary>
    public sealed record Expired : SetupTokenValidationError;

    /// <summary>
    /// Indicates that the setup token could not be found.
    /// </summary>
    public sealed record DoesNotExist : SetupTokenValidationError;

    /// <summary>
    /// Indicates that the setup token has already been used and can no longer be validated.
    /// </summary>
    public sealed record Consumed : SetupTokenValidationError;
}
