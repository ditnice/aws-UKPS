namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Defines a service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message to the specified email address.
    /// </summary>
    /// <param name="recipient">
    /// The email address of the recipient.
    /// </param>
    /// <param name="email">
    /// The email message containing the subject and content to send.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous send operation.
    /// </returns>
    Task SendEmail(string recipient, IEmail email, CancellationToken cancellationToken);
}
