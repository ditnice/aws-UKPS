using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using GetRecordsResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Records.Dtos.RecordListItemDto>,
    UKPS.Api.Application.Records.Errors.GetRecordsError
>;

namespace UKPS.Api.Application.Records;

internal partial class RecordService(
    AppDbContext dbContext,
    IDateTimeProvider timeProvider,
    IOrganisationAuthoriser organisationAuthoriser,
    CurrentDbUserEntityService currentDbUserEntityService
) : IRecordService
{
    private readonly IDateTimeProvider _timeProvider = timeProvider;

    private readonly CurrentDbUserEntityService _currentDbUserEntityService =
        currentDbUserEntityService;

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

        IQueryable<RecordInformationTrackingProjection> query = JoinProductDetails(
                GetBaseRecordProjection()
            )
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
                DisplayStatus = m.DisplayStatus,
                Title = m.Title ?? string.Empty,
                CompanyCode = m.CompanyCode,
                ReviewedAt = m.ReviewedAt,
                NextUpdateDue = m.NextUpdateDue,
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

    public async Task<Result<DiscardRecordError>> DiscardDraftRecord(
        int recordId,
        CancellationToken cancellationToken
    )
    {
        Record? record = await dbContext.Records.FirstOrDefaultAsync(
            r => r.Id == recordId,
            cancellationToken
        );
        if (record is null)
        {
            return Result<DiscardRecordError>.Err(new DiscardRecordError.RecordNotFound());
        }

        if (
            !_organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Create,
                record.OrganisationId
            )
        )
        {
            return Result<DiscardRecordError>.Err(new DiscardRecordError.NotAllowed());
        }

        var latest = await dbContext
            .Records.Where(r => r.Id == recordId)
            .SelectLatestRevision()
            .Select(x => new { x.LatestRevisionId, x.HasSingleRevision })
            .SingleAsync(cancellationToken);
        RecordRevision? latestRevision = await dbContext.RecordRevisions.FirstOrDefaultAsync(
            r => r.Id == latest.LatestRevisionId,
            cancellationToken
        );
        if (latestRevision is null)
        {
            return Result<DiscardRecordError>.Err(new DiscardRecordError.InvalidDiscard());
        }

        User currentUser = await _currentDbUserEntityService.GetCurrentUser(cancellationToken);
        bool isDiscarded = record.TryDiscard(
            latestRevision,
            latest.HasSingleRevision,
            _timeProvider.GetUtcNow(),
            currentUser,
            RecordStatusChangeReason.ManuallyDiscarded
        );
        if (!isDiscarded)
        {
            return Result<DiscardRecordError>.Err(new DiscardRecordError.InvalidDiscard());
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<DiscardRecordError>.Err(new DiscardRecordError.ConcurrencyError());
        }
        return Result<DiscardRecordError>.Ok();
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

        if (getRecordsQuery.DisplayStatus.Count > 0)
        {
            input = input.Where(m => getRecordsQuery.DisplayStatus.Contains(m.DisplayStatus));
        }

        if (getRecordsQuery.UpdateStatus.Count > 0)
        {
            bool includeOverdue = getRecordsQuery.UpdateStatus.Contains(UpdateStatus.Overdue);
            bool includeNotOverdue = getRecordsQuery.UpdateStatus.Contains(UpdateStatus.NotOverdue);

            if (includeOverdue && !includeNotOverdue)
            {
                var now = _timeProvider.GetUtcNow();
                input = input.Where(m => m.NextUpdateDue != null && m.NextUpdateDue < now);
            }
            else if (includeNotOverdue && !includeOverdue)
            {
                var now = _timeProvider.GetUtcNow();
                input = input.Where(m => m.NextUpdateDue == null || m.NextUpdateDue >= now);
            }
        }

        string? search = getRecordsQuery.Search;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{Helpers.EscapeLikePattern(search.Trim())}%";

            input = input.Where(m =>
                (m.Title != null && EF.Functions.ILike(m.Title, pattern, "\\"))
                || EF.Functions.ILike(m.CompanyCode, pattern, "\\")
#pragma warning disable CA1305, MA0011 // Translated to SQL, so locale does not apply
                || EF.Functions.ILike(m.Id.ToString(), pattern, "\\")
#pragma warning restore CA1305, MA0011
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
                GetRecordsQuerySortValue.CompanyCode => m => m.CompanyCode,
                GetRecordsQuerySortValue.DisplayStatus => m => m.DisplayStatus,
                GetRecordsQuerySortValue.RecordTitle => m => m.Title,
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
        public RecordDisplayStatus DisplayStatus { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public string? Title { get; init; }
        public required string CompanyCode { get; init; }
        public DateTime? NextUpdateDue { get; init; }
    }

    private record BaseRecordProjection
    {
        public int Id { get; init; }
        public int OrganisationId { get; init; }
        public RecordType RecordType { get; init; }
        public RecordStatus RecordStatus { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public int? LatestRevisionId { get; init; }
        public RecordDisplayStatus DisplayStatus { get; init; }
        public DateTime? NextUpdateDue { get; init; }
    }

    private IQueryable<BaseRecordProjection> GetBaseRecordProjection() =>
        dbContext
            .Records.SelectLatestRevision()
            .Select(x => new BaseRecordProjection
            {
                Id = x.Record.Id,
                OrganisationId = x.Record.OrganisationId,
                RecordType = x.Record.RecordType,
                RecordStatus = x.Record.RecordStatus,
                ReviewedAt = x.Record.ReviewedAt,
                LatestRevisionId = x.LatestRevisionId,
                DisplayStatus = x.DisplayStatus,
                NextUpdateDue =
                    x.Record.ReviewedAt == null
                        ? null
                        : x.Record.ReviewedAt.Value.AddMonths(PublishedRecordUpdateDueMonths), // TODO rules around this need to be reviewed, requires wider-team discussion
            });

    private IQueryable<RecordInformationTrackingProjection> JoinProductDetails(
        IQueryable<BaseRecordProjection> input
    ) =>
        input.Join(
            dbContext.RecordProductDetails,
            x => x.LatestRevisionId,
            y => y.RevisionId,
            (a, b) =>
                new RecordInformationTrackingProjection
                {
                    Id = a.Id,
                    OrganisationId = a.OrganisationId,
                    RecordType = a.RecordType,
                    RecordStatus = a.RecordStatus,
                    DisplayStatus = a.DisplayStatus,
                    ReviewedAt = a.ReviewedAt,
                    NextUpdateDue = a.NextUpdateDue,
                    Title = b.RecordTitle,
                    CompanyCode = b.CompanyCode,
                }
        );
}
