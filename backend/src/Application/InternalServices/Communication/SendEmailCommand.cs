using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Represents a command containing the details required to send an email.
/// </summary>
public sealed record SendEmailCommand
{
    /// <summary>
    /// A reference to the user so that we can identify the user
    /// without logging their email address.
    /// </summary>
    public required CognitoUsername CognitoUsername { get; init; }

    /// <summary>
    /// Gets the email address of the intended recipient.
    /// </summary>
    public required string RecipientAddress { get; init; }

    /// <summary>
    /// Gets the email message to send.
    /// </summary>
    public required IEmail Email { get; init; }
}
