namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Patient Access Scheme regions selected for a medicine record.
/// Multi-select — see MedicinesBudgetImpact.PatientAccessSchemeRegions.
/// </summary>
[Flags]
public enum PatientAccessSchemeRegion
{
    /// <summary>England.</summary>
    England = 1,

    /// <summary>Wales.</summary>
    Wales = 2,

    /// <summary>Scotland.</summary>
    Scotland = 4,

    /// <summary>Northern Ireland.</summary>
    NorthernIreland = 8,
}
