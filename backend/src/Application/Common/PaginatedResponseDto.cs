namespace UKPS.Api.Application.Common;

/// <summary>
/// Represents a paginated response containing a collection of items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the paginated response.</typeparam>
public sealed record PaginatedResponseDto<T>
{
    /// <summary>
    /// Gets the collection of items in the current page. This is a test. Lovely tests.
    /// </summary>
    public required IReadOnlyCollection<T> Items { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number (1-based index).
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public required int PageSize { get; init; }
}
