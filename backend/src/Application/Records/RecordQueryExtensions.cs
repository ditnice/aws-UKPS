using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Provides query extensions for records.
/// </summary>
internal static class RecordQueryExtensions
{
    /// <summary>
    /// Projects each record with its latest revision and the status shown to users. On hold and
    /// archived records show their record status; otherwise the latest revision's workflow status
    /// is shown, with a rejected revision shown as a draft since it is returned to the user to
    /// edit.
    /// </summary>
    /// <param name="records">The records to project.</param>
    /// <returns>The records with their latest revision details.</returns>
    public static IQueryable<RecordWithLatestRevision> SelectLatestRevision(
        this IQueryable<Record> records
    ) =>
        records
            .Select(r => new
            {
                Record = r,
                Latest = r
                    .Revisions.OrderByDescending(rev => rev.CreatedAt)
                    .ThenByDescending(rev => rev.Id)
                    .FirstOrDefault(),
            })
            .Select(x => new RecordWithLatestRevision
            {
                Record = x.Record,
                LatestRevisionId = x.Latest == null ? null : x.Latest.Id,
                DisplayStatus =
                    x.Record.RecordStatus == RecordStatus.OnHold ? RecordDisplayStatus.OnHold
                    : x.Record.RecordStatus == RecordStatus.Archived ? RecordDisplayStatus.Archived
                    : x.Latest != null && x.Latest.WorkflowStatus == WorkflowStatus.QAReview
                        ? RecordDisplayStatus.QAReview
                    : x.Latest != null && x.Latest.WorkflowStatus == WorkflowStatus.Published
                        ? RecordDisplayStatus.Published
                    : RecordDisplayStatus.Draft,
            });
}
