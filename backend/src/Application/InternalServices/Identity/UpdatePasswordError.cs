using Amazon.CognitoIdentityProvider.Model;

namespace UKPS.Api.Application.InternalServices.Identity;

/// <summary>
/// Represents errors that can occur when updating a user's password.
/// </summary>
public abstract record UpdatePasswordError
{
    /// <summary>
    /// Indicates that the provided password does not meet the required validation rules.
    /// </summary>
    public sealed record InvalidPassword : UpdatePasswordError;

    internal TResult Match<TResult>(Func<InvalidPassword, TResult> invalidPassword)
    {
        return this switch
        {
            InvalidPassword x => invalidPassword(x),
            _ => throw new UnsupportedOperationException("Unknown update password error."),
        };
    }
}
