namespace UKPS.Api.Enums;

public enum TermsAcceptanceStatus
{
    Pending = 0,
#pragma warning disable CA1720
    Signed = 1,
#pragma warning restore CA1720
    Expired = 2,
}
