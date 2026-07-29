namespace UKPS.Api.Application.AuthorisationAdministration;

/// <summary>
/// Represents errors that can occur during the user setup process.
/// </summary>
public abstract record UserSetupError
{
    /// <summary>
    /// Prevents direct instantiation of user setup errors.
    /// </summary>
    protected UserSetupError() { }

    /// <summary>
    /// Indicates that the setup token has already been used and cannot be used again.
    /// </summary>
    internal sealed record Consumed : UserSetupError;

    /// <summary>
    /// Indicates that the supplied password does not meet the required validation rules.
    /// </summary>
    internal sealed record InvalidPassword : UserSetupError;

    /// <summary>
    /// Indicates that the setup token has expired and is no longer valid.
    /// </summary>
    public sealed record Expired : UserSetupError;

    /// <summary>
    /// Indicates that the setup token could not be found.
    /// </summary>
    public sealed record DoesNotExist : UserSetupError;
}
