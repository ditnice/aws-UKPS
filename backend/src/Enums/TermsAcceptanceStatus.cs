namespace UKPS.Api.Enums;

internal enum TermsAcceptanceStatus
{
    Pending,
#pragma warning disable CA1720
    Signed,
#pragma warning restore CA1720
    Expired
}
