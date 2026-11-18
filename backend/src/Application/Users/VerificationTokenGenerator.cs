using System.Security.Cryptography;

namespace UKPS.Api.Application.Users;

internal static class VerificationTokenGenerator
{
    public static string Generate()
    {
        return Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
    }
}
