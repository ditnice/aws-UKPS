using System.ComponentModel.DataAnnotations;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Specifies the search, filter, pagination, and sort parameters for records.
/// </summary>
public sealed record GetRecordsQueryDto
{
    /// <summary>
    /// Gets or initialises the multi-field search term.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets or initialises the record types to include.
    /// </summary>
    public ICollection<RecordType> RecordType { get; init; } = [];

    /// <summary>
    /// Gets or initialises the record statuses to include.
    /// </summary>
    public ICollection<RecordStatus> RecordStatus { get; init; } = [];

    /// <summary>
    /// Gets or initialises the 1-based page number.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page cannot be less than 1.")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or initialises the number of records per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Gets or initialises the field by which records are sorted.
    /// </summary>
    public GetRecordsQuerySortValue SortBy { get; init; } = GetRecordsQuerySortValue.NextUpdateDue;

    /// <summary>
    /// Gets or initialises the sort direction.
    /// </summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;
}
