namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Defines a service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message to the specified email address.
    /// </summary>
    /// <param name="command">
    /// The command containing the recipient address and email message to send.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous send operation.
    /// </returns>
    Task SendEmail(SendEmailCommand command, CancellationToken cancellationToken);
}
