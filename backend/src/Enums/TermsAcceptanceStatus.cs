namespace UKPS.Api.Enums;

/// <summary>
/// Represents the status of terms acceptance for a user or entity.
/// </summary>
public enum TermsAcceptanceStatus
{
    /// <summary>
    /// Indicates that the terms have not yet been accepted.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Indicates that the terms have been accepted and signed.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name
    Signed = 1,
#pragma warning restore CA1720 // Identifier contains type name

    /// <summary>
    /// Indicates that the terms acceptance has expired and may need renewal.
    /// </summary>
    Expired = 2,
}
