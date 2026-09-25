using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Enums;
using GetRecordsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Records.Dtos.RecordListItemDto>,
    UKPS.Api.Application.Records.Errors.GetRecordsError
>;

namespace UKPS.Api.Application.Records;

internal partial class RecordService(
    AppDbContext dbContext,
    IDateTimeProvider timeProvider,
    IOrganisationAuthoriser organisationAuthoriser
) : IRecordService
{
    private readonly IDateTimeProvider _timeProvider = timeProvider;

    private readonly IOrganisationAuthoriser _organisationAuthoriser = organisationAuthoriser;

    private const int PublishedRecordUpdateDueMonths = 3;

    public async Task<GetRecordsResult> GetOrganisationRecords(
        int organisationId,
        GetRecordsQueryDto getRecordsQuery,
        CancellationToken cancellationToken
    )
    {
        GetRecordsError? organisationError = await ValidateOrganisationAsync(
            organisationId,
            Operation.Read,
            cancellationToken
        );

        if (organisationError is not null)
        {
            return GetRecordsResult.Err(organisationError);
        }

        IQueryable<RecordInformationTrackingProjection> query = GetProjectedRecordInformation()
            .Where(x => x.OrganisationId == organisationId);

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

        var items = await orderedRecords
            .Skip((getRecordsQuery.Page - 1) * getRecordsQuery.PageSize)
            .Take(getRecordsQuery.PageSize)
            .ToListAsync(cancellationToken);

        var projectedItems = items
            .Select(m => new RecordListItemDto
            {
                Id = m.Id,
                RecordType = m.RecordType,
                RecordStatus = m.RecordStatus,
                Title = m.Title ?? string.Empty,
                DevelopmentName = m.DevelopmentName,
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

    private async Task<GetRecordsError?> ValidateOrganisationAsync(
        int organisationId,
        Operation operation,
        CancellationToken cancellationToken
    )
    {
        bool actionPermitted = _organisationAuthoriser.CanPerformOperationOnOrganisation(
            operation,
            organisationId
        );

        if (!actionPermitted)
        {
            return new GetRecordsError.NotAllowed(organisationId);
        }

        bool organisationExists = await dbContext.Organisations.AnyAsync(
            o => o.Id == organisationId,
            cancellationToken
        );

        return organisationExists ? null : new GetRecordsError.OrganisationNotFound(organisationId);
    }

    private IQueryable<RecordInformationTrackingProjection> GetProjectedRecordInformation()
    {
        var medicines = JoinMedicinesProductDetails(GetBaseRecordProjection());
        var vaccines = JoinVaccinesProductDetails(GetBaseRecordProjection());

        return medicines.Union(vaccines);
    }

    private IQueryable<RecordInformationTrackingProjection> ApplyFilters(
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
            var now = _timeProvider.GetUtcNow();

            input = getRecordsQuery.UpdateStatus.Value switch
            {
                UpdateStatus.Overdue => input.Where(m =>
                    m.NextUpdateDue != null && m.NextUpdateDue < now
                ),
                UpdateStatus.NotOverdue => input.Where(m =>
                    m.NextUpdateDue == null || m.NextUpdateDue >= now
                ),
                _ => input,
            };
        }

        string? search = getRecordsQuery.Search;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{Helpers.EscapeLikePattern(search.Trim())}%";

            input = input.Where(m =>
                (m.Title != null && EF.Functions.ILike(m.Title, pattern, "\\"))
                || (
                    m.DevelopmentName != null
                    && EF.Functions.ILike(m.DevelopmentName, pattern, "\\")
                )
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
                GetRecordsQuerySortValue.DevelopmentName => m => m.DevelopmentName,
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

    public record RecordInformationTrackingProjection
    {
        public int Id { get; init; }
        public int OrganisationId { get; init; }
        public RecordType RecordType { get; init; }
        public RecordStatus RecordStatus { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public string? Title { get; init; }
        public string? DevelopmentName { get; init; }
        public DateTime? NextUpdateDue { get; init; }
    }

    private record BaseRecordProjection
    {
        public int Id { get; init; }
        public int OrganisationId { get; init; }
        public RecordType RecordType { get; init; }
        public RecordStatus RecordStatus { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public int? CurrentDraftRevisionId { get; init; }
        public DateTime? NextUpdateDue { get; init; }
    }

    private IQueryable<BaseRecordProjection> GetBaseRecordProjection() =>
        dbContext.Records.Select(x => new BaseRecordProjection
        {
            Id = x.Id,
            OrganisationId = x.OrganisationId,
            RecordType = x.RecordType,
            RecordStatus = x.RecordStatus,
            ReviewedAt = x.ReviewedAt,
            CurrentDraftRevisionId = x.CurrentDraftRevisionId,
            NextUpdateDue =
                x.ReviewedAt == null
                    ? null
                    : x.ReviewedAt.Value.AddMonths(PublishedRecordUpdateDueMonths), // TODO rules around this need to be reviewed, requires wider-team discussion
        });

    private IQueryable<RecordInformationTrackingProjection> JoinMedicinesProductDetails(
        IQueryable<BaseRecordProjection> input
    ) =>
        input.Join(
            dbContext.MedicinesProductDetails,
            x => x.CurrentDraftRevisionId,
            y => y.RevisionId,
            (a, b) =>
                new RecordInformationTrackingProjection
                {
                    Id = a.Id,
                    OrganisationId = a.OrganisationId,
                    RecordType = a.RecordType,
                    RecordStatus = a.RecordStatus,
                    ReviewedAt = a.ReviewedAt,
                    NextUpdateDue = a.NextUpdateDue,
                    Title = b.RecordTitle,
                    DevelopmentName = b
                        .ActiveSubstances.OrderBy(x => x.DisplayOrder)
                        .First(x => x.NameType == SubstanceNameType.DevelopmentName)
                        .Name,
                }
        );

    private IQueryable<RecordInformationTrackingProjection> JoinVaccinesProductDetails(
        IQueryable<BaseRecordProjection> input
    ) =>
        input.Join(
            dbContext.VaccinesProductDetails,
            x => x.CurrentDraftRevisionId,
            y => y.RevisionId,
            (x, details) =>
                new RecordInformationTrackingProjection
                {
                    Id = x.Id,
                    OrganisationId = x.OrganisationId,
                    RecordType = x.RecordType,
                    RecordStatus = x.RecordStatus,
                    ReviewedAt = x.ReviewedAt,
                    NextUpdateDue = x.NextUpdateDue,
                    Title = details.RecordTitle,
                    DevelopmentName = details.CompanyCode,
                }
        );
}
