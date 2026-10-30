namespace UKPS.Api.Application.InternalServices.Hosting;

/// <summary>
/// Defines a contract for creating password reset links.
/// </summary>
public interface IPasswordResetLinkCreator
{
    /// <summary>
    /// Builds the absolute URI a user can follow to reset their password.
    /// </summary>
    /// <param name="token">
    /// The unique password reset token that identifies and authorizes the reset request.
    /// </param>
    /// <returns>
    /// A <see cref="Uri"/> containing the password reset link, with the specified token embedded.
    /// </returns>
    Uri GetPasswordResetLink(string token);
}
