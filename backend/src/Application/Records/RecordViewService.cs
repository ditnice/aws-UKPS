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
        var (record, error) = await FindRecord(
            recordId,
            recordType,
            organisationId,
            cancellationToken
        );
        return record is null
            ? GetRecordOverviewResult.Err(error!)
            : GetRecordOverviewResult.Ok(await ToOverview(record, cancellationToken));
    }

    public async Task<GetRecordDetailResult> GetRecordDetail(
        int recordId,
        RecordType recordType,
        int organisationId,
        CancellationToken cancellationToken
    )
    {
        var (record, error) = await FindRecord(
            recordId,
            recordType,
            organisationId,
            cancellationToken
        );
        if (record is null)
        {
            return GetRecordDetailResult.Err(error!);
        }

        return GetRecordDetailResult.Ok(
            new RecordDetailDto
            {
                Overview = await ToOverview(record, cancellationToken),
                Sections = await GetSections(record, cancellationToken),
            }
        );
    }

    /// <summary>
    /// Finds the record and its latest revision, checking the caller can read it and that it is of
    /// the requested type.
    /// </summary>
    private async Task<(RecordHeader? Record, GetRecordError? Error)> FindRecord(
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
            return (null, new GetRecordError.NotAllowed(recordId));
        }

        RecordHeader? record = await GetRecordHeader(recordId, cancellationToken);

        // Every record is created with a revision, so a missing one only guards against bad data.
        if (
            record is null
            || record.OrganisationId != organisationId
            || record.LatestRevisionId is null
            || record.LatestRevisionStatus is null
        )
        {
            return (null, new GetRecordError.NotFound(recordId));
        }

        return record.RecordType == recordType
            ? (record, null)
            : (
                null,
                new GetRecordError.RecordTypeMismatch(recordId, recordType, record.RecordType)
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
            record.LatestRevisionId!.Value,
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

    private Task<RecordHeader?> GetRecordHeader(
        int recordId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Records.AsNoTracking()
            .Where(r => r.Id == recordId)
            .SelectLatestRevision()
            .Select(x => new RecordHeader(
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
                new(record.RecordStatus, record.LatestRevisionStatus!.Value),
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

    private sealed record RecordHeader(
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
}
