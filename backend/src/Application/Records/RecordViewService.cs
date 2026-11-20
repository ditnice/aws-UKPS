using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

using GetRecordDetailResult = Result<RecordDetailDto, GetRecordError>;
using GetRecordOverviewResult = Result<RecordOverviewDto, GetRecordError>;

internal sealed class RecordViewService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser,
    ICurrentUserInfoService currentUserInfoService,
    FormModelRegistry formModels,
    RecordFormPageLoader formLoader
) : IRecordViewService
{
    public async Task<GetRecordOverviewResult> GetRecordOverview(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        Result<RecordHeader, GetRecordError> found = await FindRecord(
            recordId,
            recordType,
            organisationId,
            cancellationToken
        );
        return await found.Match(
            onOk: async record =>
                GetRecordOverviewResult.Ok(await ToOverview(record, cancellationToken)),
            onErr: static error => Task.FromResult(GetRecordOverviewResult.Err(error))
        );
    }

    public async Task<GetRecordDetailResult> GetRecordDetail(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        Result<RecordHeader, GetRecordError> found = await FindRecord(
            recordId,
            recordType,
            organisationId,
            cancellationToken
        );
        return await found.Match(
            onOk: async record =>
                GetRecordDetailResult.Ok(
                    new RecordDetailDto
                    {
                        Overview = await ToOverview(record, cancellationToken),
                        Sections = await GetSections(record, cancellationToken),
                    }
                ),
            onErr: static error => Task.FromResult(GetRecordDetailResult.Err(error))
        );
    }

    /// <summary>
    /// Finds the record and its latest revision, checking the caller can read it and that it is of
    /// the requested type.
    /// </summary>
    private async Task<Result<RecordHeader, GetRecordError>> FindRecord(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        currentUserInfoService.ThrowIfOrganisationNotSelected(organisationId);

        // Authorise against the requested organisation before looking the record up, so an
        // unauthorised caller gets the same response whether or not the record exists.
        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                organisationId
            )
        )
        {
            return Result<RecordHeader, GetRecordError>.Err(
                new GetRecordError.NotAllowed(recordId)
            );
        }

        RecordHeaderRow? row = await GetRecordHeaderRow(recordId, cancellationToken);

        // Every record is created with a revision, so a missing one only guards against bad data.
        if (
            row
                is not {
                    LatestRevisionId: int latestRevisionId,
                    LatestRevisionStatus: WorkflowStatus latestRevisionStatus,
                }
            || row.OrganisationId != organisationId
        )
        {
            return Result<RecordHeader, GetRecordError>.Err(new GetRecordError.NotFound(recordId));
        }

        if (row.RecordType != recordType)
        {
            return Result<RecordHeader, GetRecordError>.Err(
                new GetRecordError.RecordTypeMismatch(recordId, recordType, row.RecordType)
            );
        }

        return Result<RecordHeader, GetRecordError>.Ok(
            new RecordHeader(
                row.Id,
                row.OrganisationId,
                row.RecordType,
                row.RecordStatus,
                row.ReviewedAt,
                latestRevisionId,
                row.DisplayStatus,
                latestRevisionStatus,
                row.HasSingleRevision,
                row.NextUpdateDue
            )
        );
    }

    /// <summary>
    /// Every question that applies on the latest revision with its answer, built as check your
    /// answers builds them but without checking them; empty when the record type has no form.
    /// </summary>
    private async Task<IReadOnlyList<RecordAnswersSectionDto>> GetSections(
        RecordHeader record,
        CancellationToken cancellationToken
    )
    {
        FormModel? model = formModels.Find(record.RecordType);
        if (model is null)
        {
            return [];
        }

        var (answers, applicability) = await formLoader.LoadAllAnswersAsync(
            model,
            record.LatestRevisionId,
            cancellationToken
        );
        var options = await formLoader.LoadAllOptionsAsync(
            model,
            answers,
            applicability,
            cancellationToken
        );
        return new RecordAnswersBuilder(model, answers, applicability, options).Build();
    }

    private Task<RecordHeaderRow?> GetRecordHeaderRow(
        int recordId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Records.AsNoTracking()
            .Where(r => r.Id == recordId)
            .SelectLatestRevision()
            .Select(x => new RecordHeaderRow(
                x.Record.Id,
                x.Record.OrganisationId,
                x.Record.RecordType,
                x.Record.RecordStatus,
                x.Record.ReviewedAt,
                x.LatestRevisionId,
                x.DisplayStatus,
                x.LatestRevisionStatus,
                x.HasSingleRevision,
                x.NextUpdateDue
            ))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<RecordOverviewDto> ToOverview(
        RecordHeader record,
        CancellationToken cancellationToken
    )
    {
        var productDetail = await dbContext
            .RecordProductDetails.AsNoTracking()
            .Where(x => x.RevisionId == record.LatestRevisionId)
            .Select(static x => new { x.CompanyCode, x.RecordTitle })
            .SingleOrDefaultAsync(cancellationToken);

        return new()
        {
            RecordId = record.Id,
            OrganisationId = record.OrganisationId,
            RecordType = record.RecordType,
            RecordStatus = record.RecordStatus,
            DisplayStatus = record.DisplayStatus,
            ReviewedAt = record.ReviewedAt,
            CanEditContent = CanEditContent(record),
            Actions = Record.GetPermittedActions(
                new(record.RecordStatus, record.LatestRevisionStatus),
                record.HasSingleRevision,
                organisationAuthoriser.CanPerformOperationOnOrganisation(
                    Operation.ChangeRecordStatus,
                    record.OrganisationId
                )
            ),
            NextUpdateDue = record.NextUpdateDue,
            CompanyCode = productDetail?.CompanyCode,
            RecordTitle = productDetail?.RecordTitle,
        };
    }

    /// <summary>
    /// Whether the record type has a content form and the caller can edit the record's content.
    /// </summary>
    private bool CanEditContent(RecordHeader record) =>
        formModels.Find(record.RecordType) is not null
        && organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.EditRecordContent,
            record.OrganisationId
        );

    /// <summary>The record and its latest revision, as read from the database.</summary>
    private sealed record RecordHeaderRow(
        int Id,
        int OrganisationId,
        RecordType RecordType,
        RecordStatus RecordStatus,
        DateTime? ReviewedAt,
        int? LatestRevisionId,
        RecordDisplayStatus DisplayStatus,
        WorkflowStatus? LatestRevisionStatus,
        bool HasSingleRevision,
        DateTime? NextUpdateDue
    );

    /// <summary>A record the caller can read, with its latest revision.</summary>
    private sealed record RecordHeader(
        int Id,
        int OrganisationId,
        RecordType RecordType,
        RecordStatus RecordStatus,
        DateTime? ReviewedAt,
        int LatestRevisionId,
        RecordDisplayStatus DisplayStatus,
        WorkflowStatus LatestRevisionStatus,
        bool HasSingleRevision,
        DateTime? NextUpdateDue
    );
}
