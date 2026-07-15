namespace UKPS.Api.DTOs;

public sealed record PaginatedResponseDto<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
