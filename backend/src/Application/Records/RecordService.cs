using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;
using GetRecordsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Records.Dtos.RecordListItemDto>,
    UKPS.Api.Application.Records.Errors.GetRecordsError
>;

namespace UKPS.Api.Application.Records;

internal partial class RecordService(AppDbContext dbContext) : IRecordService
{
    public async Task<GetRecordsResult> GetRecords(
        GetRecordsQueryDto getRecordsQuery,
        CancellationToken cancellationToken
    )
    {
        IQueryable<RecordInformationTrackingProjection> query = GetProjectedRecordInformation();

        IQueryable<RecordInformationTrackingProjection> filteredRecords = ApplyFilters(
            query,
            getRecordsQuery
        );

        int totalCount = await filteredRecords.CountAsync(cancellationToken);

        IQueryable<RecordInformationTrackingProjection> orderedRecords = Sort(
            filteredRecords,
            getRecordsQuery.SortBy,
            getRecordsQuery.SortDirection
        );

        var Items = await orderedRecords
            .AsNoTracking()
            .Skip((getRecordsQuery.Page - 1) * getRecordsQuery.PageSize)
            .Take(getRecordsQuery.PageSize)
            .ToListAsync(cancellationToken);

        var projectedItems = Items
            .Select(m => new RecordListItemDto
            {
                Id = m.Id,
                RecordType = m.RecordType,
                RecordStatus = m.RecordStatus,
                Title = m.Title ?? string.Empty,
                NiceTaDevelopmentId = m.NiceTaDevelopmentId,
                ReviewedAt = m.ReviewedAt,
            })
            .ToArray();

        return GetRecordsResult.Ok(
            new PaginatedResponseDto<RecordListItemDto>
            {
                Items = projectedItems,
                TotalCount = totalCount,
                Page = getRecordsQuery.Page,
                PageSize = getRecordsQuery.PageSize,
            }
        );
    }

    private IQueryable<RecordInformationTrackingProjection> GetProjectedRecordInformation()
    {
        var recordProjection = dbContext.Records.Select(x => new
        {
            x.Id,
            x.OrganisationId,
            x.RecordType,
            x.RecordStatus,
            x.ReviewedAt,
            x.CurrentDraftRevisionId,
            NextUpdateDue = x.ReviewedAt == null
                ? (DateTime?)null
                : x.ReviewedAt.Value.AddMonths(3),
        });

        return recordProjection
            .GroupJoin(
                dbContext.MedicinesProductDetails,
                x => x.CurrentDraftRevisionId,
                y => y.RevisionId,
                (x, details) => new { x, details }
            )
            .SelectMany(
                x => x.details.DefaultIfEmpty(),
                (a, b) =>
                    new RecordInformationTrackingProjection
                    {
                        Id = a.x.Id,
                        OrganisationId = a.x.OrganisationId,
                        RecordType = a.x.RecordType,
                        RecordStatus = a.x.RecordStatus,
                        ReviewedAt = a.x.ReviewedAt,
                        Title = b != null ? b.RecordTitle : null,
                        NiceTaDevelopmentId = b != null ? b.NiceTaDevelopmentId : null,
                        NextUpdateDue = a.x.NextUpdateDue,
                    }
            );
    }

    private static IQueryable<RecordInformationTrackingProjection> ApplyFilters(
        IQueryable<RecordInformationTrackingProjection> input,
        GetRecordsQueryDto getRecordsQuery
    )
    {
        if (getRecordsQuery.RecordType.Count > 0)
        {
            input = input.Where(m => getRecordsQuery.RecordType.Contains(m.RecordType));
        }

        if (getRecordsQuery.RecordStatus.Count > 0)
        {
            input = input.Where(m => getRecordsQuery.RecordStatus.Contains(m.RecordStatus));
        }

        if (getRecordsQuery.UpdateStatus.HasValue)
        {
            input = getRecordsQuery.UpdateStatus.Value switch
            {
                UpdateStatus.Overdue => input.Where(m =>
                    m.NextUpdateDue != null && m.NextUpdateDue < DateTime.UtcNow
                ),
                UpdateStatus.NotOverdue => input.Where(m =>
                    m.NextUpdateDue == null || m.NextUpdateDue >= DateTime.UtcNow
                ),
                _ => input,
            };
        }

        if (!string.IsNullOrWhiteSpace(getRecordsQuery.Search))
        {
            string pattern = $"%{EscapeLikePattern(getRecordsQuery.Search)}%";
            bool isNumeric = int.TryParse(
                getRecordsQuery.Search.Trim(),
                System.Globalization.CultureInfo.InvariantCulture,
                out int searchId
            );

            input = input.Where(m =>
                (m.Title != null && EF.Functions.ILike(m.Title, pattern, "\\"))
                || (
                    m.NiceTaDevelopmentId != null
                    && EF.Functions.ILike(m.NiceTaDevelopmentId, pattern, "\\")
                )
                || (isNumeric && m.Id == searchId)
            );
        }

        return input;
    }

    private static IQueryable<RecordInformationTrackingProjection> Sort(
        IQueryable<RecordInformationTrackingProjection> value,
        GetRecordsQuerySortValue sortBy,
        SortDirection sortDirection
    )
    {
        Expression<Func<RecordInformationTrackingProjection, object?>> sortExpression =
            sortBy switch
            {
                GetRecordsQuerySortValue.NextUpdateDue => m =>
                    m.NextUpdateDue == null ? DateTime.MaxValue : m.NextUpdateDue.Value,
                GetRecordsQuerySortValue.Id => m => m.Id,
                GetRecordsQuerySortValue.DevelopmentName => m => m.NiceTaDevelopmentId,
                GetRecordsQuerySortValue.RecordStatus => m => m.RecordStatus,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(sortBy),
                    $"Unexpected value: {sortBy}"
                ),
            };

        return sortDirection switch
        {
            SortDirection.Ascending => value.OrderBy(sortExpression).ThenBy(x => x.Id),
            SortDirection.Descending => value
                .OrderByDescending(sortExpression)
                .ThenByDescending(x => x.Id),
            _ => throw new ArgumentOutOfRangeException(
                nameof(sortDirection),
                $"Unexpected value: {sortDirection}"
            ),
        };
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    public record RecordInformationTrackingProjection
    {
        public int Id { get; init; }
        public int OrganisationId { get; init; }
        public RecordType RecordType { get; init; }
        public RecordStatus RecordStatus { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public string? Title { get; init; }
        public string? NiceTaDevelopmentId { get; init; }
        public DateTime? NextUpdateDue { get; init; }
    }
}
