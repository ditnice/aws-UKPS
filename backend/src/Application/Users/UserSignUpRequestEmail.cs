using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserSignUpRequestEmail : IEmail
{
    public string Subject => "UKPS Sign Up Request";
    public string Content =>
        $"""
Hello
You have been invited to register for UK PharmaScan by your organisation's champion user.
To access UK PharmaScan you now need to activate your account.
To activate your account please click on the following link:

{Link}

Should you experience any problems activating your account then please contact the UK PharmaScan helpdesk by emailing {HelpdeskEmail}
""";
    public required string Link { get; init; }
    public string HelpdeskEmail { get; init; } = "**Placeholder**";
}
