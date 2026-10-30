using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Authentication;

/// <summary>
/// Represents the email sent to a user who has requested a password reset.
/// </summary>
public record PasswordResetEmail : IEmail
{
    /// <summary>
    /// Gets the absolute URI the recipient follows to reset their password.
    /// </summary>
    public required Uri Link { get; init; }

    /// <summary>
    /// Gets the subject line of the email.
    /// </summary>
    public string Subject => "UKPS Reset your password";

    /// <summary>
    /// Builds the HTML body of the password reset email.
    /// </summary>
    /// <param name="contextData">
    /// Contextual data for rendering the email. Not used by this email type.
    /// </param>
    /// <returns>
    /// An HTML string containing the reset request message and a link to <see cref="Link"/>.
    /// The link is HTML-encoded before being embedded in the markup.
    /// </returns>
    public string GetHtmlContent(EmailContextData contextData)
    {
        var link = System.Net.WebUtility.HtmlEncode(Link.AbsoluteUri);
        return $"""
<p>We received a request to reset the password for your account.</p>
<p><a href="{link}">Reset your password</a></p>
<p>If you did not request a password reset, you can ignore this email.</p>
""";
    }
}
