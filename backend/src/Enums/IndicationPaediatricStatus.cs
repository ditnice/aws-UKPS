namespace UKPS.Api.Enums;

/// <summary>
/// Represents the paediatric status of an indication, specifying the target age group for a treatment or condition.
/// </summary>
public enum IndicationPaediatricStatus
{
    /// <summary>
    /// Indicates that the treatment or condition is exclusively for children.
    /// </summary>
    ExclusivelyChildren = 0,

    /// <summary>
    /// Indicates that the treatment or condition is for both children and adults.
    /// </summary>
    BothChildrenAndAdults = 1,

    /// <summary>
    /// Indicates that the treatment or condition is exclusively for adults.
    /// </summary>
    ExclusivelyAdults = 2,

    /// <summary>
    /// Indicates that the paediatric status of the treatment or condition is unknown.
    /// </summary>
    Unknown = 3,
}
