namespace UKPS.Api.Enums;

internal enum RecordEventType
{
    RecordCreated,
    RecordStatusChanged,
    DraftCreated,
    DraftSaved,
    DraftSubmitted,
    DraftSuperseded,
    QaReviewCreated,
    QaReviewCompleted,
    QaIssueAdded,
    QaIssueResolved,
    QaIssueReopened,
    RevisionRejected,
    RevisionPublished,
    RecordReviewedNoChange
}
