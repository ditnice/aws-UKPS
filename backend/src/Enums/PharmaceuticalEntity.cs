namespace UKPS.Api.Enums;

/// <summary>
/// Represents the types of pharmaceutical entities.
/// </summary>
[Flags]
public enum PharmaceuticalEntity
{
    /// <summary>
    /// Represents medicines as a pharmaceutical entity.
    /// </summary>
    Medicines = 1,

    /// <summary>
    /// Represents vaccines as a pharmaceutical entity.
    /// </summary>
    Vaccines = 2,

    /// <summary>
    /// Represents both medicines and vaccines as pharmaceutical entities.
    /// </summary>
    Both = Medicines | Vaccines,
}
