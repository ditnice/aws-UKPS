using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal sealed record EmailChangeVerificationRequestEmail : IEmail
{
    public string Subject => "New Email Verification Required";
    public required Uri EmailVerificationLink { get; init; }

    public string GetHtmlContent(EmailContextData contextData)
    {
        return $"""
            <p>We received a request to change the email address on your account to this address.</p>
            <p>To confirm this change, please verify your new email address by clicking the link below:</p>
            <p><a href="{EmailVerificationLink}">Verify my new email address</a></p>
            <p>If you didn't request this change, you can safely ignore this email and your email address will not be changed. If you're concerned about the security of your account, please contact support at {contextData.HelpDeskEmailLink}.</p>
            """;
    }
}
