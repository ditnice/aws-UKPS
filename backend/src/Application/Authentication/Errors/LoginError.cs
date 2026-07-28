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

    internal sealed record NewPasswordRequired : LoginError;

    /// <summary>
    /// Represents an error indicating that the supplied credentials were not authorised.
    /// </summary>
    public sealed record Unauthorised : LoginError;
}
