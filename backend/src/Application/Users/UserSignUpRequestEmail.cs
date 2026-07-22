using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Application.Users;

internal class UserSignUpRequestEmail : IEmail
{
    public string Subject => "UKPS Sign Up Request";
    public string Content =>
        $"Hello, someone wants you to create an account. Click this link: {Link}";
    public required string Link { get; init; }
}
