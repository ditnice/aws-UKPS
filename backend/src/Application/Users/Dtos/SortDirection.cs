namespace UKPS.Api.Application.Users.Dtos;

/// <summary>
/// Specifies the direction in which query results are sorted.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Sorts values from lowest to highest, or alphabetically from A to Z.
    /// </summary>
    Ascending = 0,

    /// <summary>
    /// Sorts values from highest to lowest, or alphabetically from Z to A.
    /// </summary>
    Descending = 1,
}
