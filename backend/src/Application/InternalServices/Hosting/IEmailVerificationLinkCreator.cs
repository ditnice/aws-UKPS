namespace UKPS.Api.Application.InternalServices.Hosting;

/// <summary>
/// Provides an abstraction for creating email verification links.
/// </summary>
public interface IEmailVerificationLinkCreator
{
    /// <summary>
    /// Creates a verification link associated with the specified verification token.
    /// </summary>
    /// <param name="verificationToken">
    /// The unique token used to identify and validate the email verification request.
    /// </param>
    /// <returns>
    /// A URL that allows the user to verify their email address.
    /// </returns>
    Uri GetVerificationLink(string verificationToken);
}
