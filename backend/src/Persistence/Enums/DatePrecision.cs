namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the level of precision for a date.
/// </summary>
public enum DatePrecision
{
    /// <summary>
    /// The date is estimated to the quarter of the year.
    /// </summary>
    EstimatedQuarter = 0,

    /// <summary>
    /// The date is estimated to the month.
    /// </summary>
    EstimatedMonth = 1,

    /// <summary>
    /// The date is an exact, specific day.
    /// </summary>
    ActualDate = 2,
}
