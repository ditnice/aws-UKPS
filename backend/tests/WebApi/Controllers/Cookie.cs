using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace UKPS.Api.Tests.WebApi.Controllers;

internal sealed record Cookie
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public DateTimeOffset? Expires { get; init; }
    public bool Secure { get; init; }
    public bool HttpOnly { get; init; }
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Unspecified;
    public string Path { get; init; } = "/";

    public static Cookie Parse(string value)
    {
        var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        return parts.Aggregate(
            new Cookie(),
            (cookie, part) =>
            {
                if (part.Contains('=', StringComparison.Ordinal))
                {
                    string[] keyValue = part.Split('=', 2);
                    string key = keyValue[0].Trim();
                    string val = keyValue[1].Trim();

                    if (
                        string.Equals(key, "expires", StringComparison.OrdinalIgnoreCase)
                        && DateTimeOffset.TryParse(
                            val,
                            CultureInfo.InvariantCulture,
                            out var expires
                        )
                    )
                    {
                        return cookie with { Expires = expires };
                    }
                    if (
                        string.Equals(key, "samesite", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(val, "strict", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return cookie with { SameSite = SameSiteMode.Strict };
                    }
                    if (string.Equals(key, "path", StringComparison.OrdinalIgnoreCase))
                    {
                        return cookie with { Path = val };
                    }

                    return cookie with
                    {
                        Name = key,
                        Value = val,
                    };
                }
                else
                {
                    if (string.Equals(part, "secure", StringComparison.OrdinalIgnoreCase))
                    {
                        return cookie with { Secure = true };
                    }
                    if (string.Equals(part, "httponly", StringComparison.OrdinalIgnoreCase))
                    {
                        return cookie with { HttpOnly = true };
                    }
                }

                return cookie;
            }
        );
    }
}
