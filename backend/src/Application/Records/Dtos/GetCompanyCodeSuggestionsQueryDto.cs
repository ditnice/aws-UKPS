using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// Specifies the search parameters for company code suggestions.
/// </summary>
public sealed record GetCompanyCodeSuggestionsQueryDto
{
    /// <summary>
    /// Gets or initialises the text the company codes must contain, ignoring case. A blank search
    /// returns no suggestions.
    /// </summary>
    [MaxLength(200, ErrorMessage = "Search must be 200 characters or fewer.")]
    public string? Search { get; init; }

    /// <summary>
    /// Gets or initialises the maximum number of suggestions to return.
    /// </summary>
    [Range(1, 20, ErrorMessage = "Limit must be between 1 and 20.")]
    public int Limit { get; init; } = 10;
}
