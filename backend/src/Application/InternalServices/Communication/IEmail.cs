namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Represents the content of an email message.
/// </summary>
public interface IEmail
{
    /// <summary>
    /// Gets the subject of the email message.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Gets the body content of the email message.
    /// </summary>
    string Content { get; }
}
