namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// PAS (Patient Access Scheme) regions selected for a medicine record.
/// Multi-select — see MedicinesBudgetImpact.PasRegions.
/// </summary>
[Flags]
public enum PasRegion
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
