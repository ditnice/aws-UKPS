namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// 4 values — medicines only:
/// Initial presentation, Progression, Longitudinal testing, Other.
/// </summary>
internal sealed class PatientPathwayPoint : ReferenceDataBase
{
    /// <summary>
    /// A stable key for the pathway point that never changes with the label, so form conditions can
    /// name it. One of <see cref="PatientPathwayPointCodes"/>.
    /// </summary>
    public required string Code { get; set; }
}
