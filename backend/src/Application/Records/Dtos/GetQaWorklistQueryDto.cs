using System.ComponentModel.DataAnnotations;
using UKPS.Api.Application.Common;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Specifies the search, filter, pagination, and sort parameters for the QA worklist.
/// </summary>
public sealed record GetQaWorklistQueryDto
{
    /// <summary>
    /// Gets or initialises the multi-field search term, matched against the record title, company
    /// code and record ID.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets or initialises the record types to include. When empty, all types are returned.
    /// </summary>
    public ICollection<QaRecordType> QaRecordType { get; init; } = [];

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
    /// Gets or initialises the field by which records are sorted. Defaults to the time submitted
    /// to QA.
    /// </summary>
    public GetQaWorklistSortValue SortBy { get; init; } = GetQaWorklistSortValue.SubmittedToQa;

    /// <summary>
    /// Gets or initialises the sort direction. Defaults to newest first.
    /// </summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Descending;
}
