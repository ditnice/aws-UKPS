using System.Security.Cryptography;
using System.Text;

namespace UKPS.Api.Application.Users;

internal static class Tokeniser
{
    public static string Generate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexStringLower(hash).Substring(0, 32);
    }
}
