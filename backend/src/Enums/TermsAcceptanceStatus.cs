namespace UKPS.Api.Enums;

public enum TermsAcceptanceStatus
{
    Pending,
#pragma warning disable CA1720
    Signed,
#pragma warning restore CA1720
    Expired
}
