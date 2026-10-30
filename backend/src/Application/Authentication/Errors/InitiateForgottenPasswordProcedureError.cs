namespace UKPS.Api.Application.Authentication.Errors;

/// <summary>
/// Represents the base type for errors that can occur when initiating the
/// forgotten password flow.
/// </summary>
public abstract record InitiateForgottenPasswordProcedureError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InitiateForgottenPasswordProcedureError"/>
    /// record. The constructor is protected to restrict derivation to the
    /// known error types defined within this assembly.
    /// </summary>
    private protected InitiateForgottenPasswordProcedureError() { }

    /// <summary>
    /// Indicates that the forgotten password flow could not be initiated
    /// because the permitted number of attempts has been exceeded.
    /// </summary>
    internal sealed record NumberOfAttemptsExceeded : InitiateForgottenPasswordProcedureError;
}
