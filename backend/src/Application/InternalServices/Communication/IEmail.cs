using System.Text.Json.Serialization;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Users;

namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Represents the content of an email message.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(UserSignUpRequestEmail), "UserSignUpRequest")]
[JsonDerivedType(typeof(ReactivatedUserNotificationEmail), "ReactivatedUserNotification")]
[JsonDerivedType(typeof(DeactivatedUserNotificationEmail), "DeactivatedUserNotification")]
public interface IEmail
{
    /// <summary>
    /// Gets the subject of the email message.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Gets the body content of the email message in HTML.
    /// </summary>
    string GetHtmlContent();
}
