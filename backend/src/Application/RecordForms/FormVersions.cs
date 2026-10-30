using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// The version of each record type's form. Clients echo it on save, and a mismatch is rejected,
/// so bump it whenever any form attribute changes. <c>MedicineFormModelTests</c> pins the
/// model's fingerprint next to it and fails until you do.
/// </summary>
internal static class FormVersions
{
    public const string Medicine = "2026.10.4";

    public static string? For(RecordType recordType) =>
        recordType switch
        {
            RecordType.Medicine => Medicine,
            _ => null,
        };
}
