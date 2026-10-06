using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// A record with its revisions, events and status history, plus the content entities of every
/// revision (which are not reachable from the record's navigation properties).
/// </summary>
internal sealed record SeededRecord(Record Record, IReadOnlyList<object> RevisionContent);
