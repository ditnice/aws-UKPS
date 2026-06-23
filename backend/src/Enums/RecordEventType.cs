namespace UKPS.Api.Enums;

public enum RecordEventType
{
    RecordCreated = 0,
    RecordStatusChanged = 1,
    DraftCreated = 2,
    DraftSaved = 3,
    DraftSubmitted = 4,
    DraftSuperseded = 5,
    QaReviewCreated = 6,
    QaReviewCompleted = 7,
    QaIssueAdded = 8,
    QaIssueResolved = 9,
    QaIssueReopened = 10,
    RevisionRejected = 11,
    RevisionPublished = 12,
    RecordReviewedNoChange = 13,
}
