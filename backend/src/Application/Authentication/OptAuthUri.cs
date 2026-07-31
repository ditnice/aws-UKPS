namespace UKPS.Api.Application.Authentication;

internal sealed record OptAuthUri(string UserEmail, string Secret)
{
    public const string Issuer = "UKPS";
    private const string Algorithm = "SHA1";
    private const int Digits = 6;
    private const int Period = 30;

    public Uri ToUri()
    {
        string label = Uri.EscapeDataString($"{Issuer}:{UserEmail}");

        return new Uri(
            $"otpauth://totp/{label}"
                + $"?secret={Uri.EscapeDataString(Secret)}"
                + $"&issuer={Uri.EscapeDataString(Issuer)}"
                + $"&algorithm={Algorithm}"
                + $"&digits={Digits}"
                + $"&period={Period}"
        );
    }
}
