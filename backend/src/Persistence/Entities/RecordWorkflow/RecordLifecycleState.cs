using System.Runtime.InteropServices;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// The status of a record together with the workflow status of its latest revision.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct RecordLifecycleState
{
    private static readonly HashSet<(RecordStatus, WorkflowStatus)> _validStates =
    [
        (RecordStatus.Unpublished, WorkflowStatus.Draft),
        (RecordStatus.Published, WorkflowStatus.Draft),
        (RecordStatus.OnHold, WorkflowStatus.Draft),
        (RecordStatus.Archived, WorkflowStatus.Draft),
        (RecordStatus.Published, WorkflowStatus.Published),
        (RecordStatus.OnHold, WorkflowStatus.Published),
        (RecordStatus.Archived, WorkflowStatus.Published),
        (RecordStatus.Unpublished, WorkflowStatus.QAReview),
        (RecordStatus.Published, WorkflowStatus.QAReview),
        (RecordStatus.Unpublished, WorkflowStatus.Rejected),
        (RecordStatus.Published, WorkflowStatus.Rejected),
        (RecordStatus.OnHold, WorkflowStatus.Rejected),
        (RecordStatus.Archived, WorkflowStatus.Rejected),
        (RecordStatus.Discarded, WorkflowStatus.Discarded),
    ];

    /// <summary>Creates a lifecycle state from statuses that can coexist.</summary>
    /// <param name="recordStatus">The record's status.</param>
    /// <param name="latestRevisionStatus">The workflow status of the record's latest revision.</param>
    /// <exception cref="ArgumentException">The statuses can't coexist.</exception>
    public RecordLifecycleState(RecordStatus recordStatus, WorkflowStatus latestRevisionStatus)
    {
        Validate(recordStatus, latestRevisionStatus);
        RecordStatus = recordStatus;
        LatestRevisionStatus = latestRevisionStatus;
    }

    /// <summary>The record's status.</summary>
    public RecordStatus RecordStatus { get; }

    /// <summary>The workflow status of the record's latest revision.</summary>
    public WorkflowStatus LatestRevisionStatus { get; }

    /// <summary>Every combination of record status and latest revision status that can coexist.</summary>
    internal static IEnumerable<RecordLifecycleState> ValidStates =>
        _validStates.Select(x => new RecordLifecycleState(x.Item1, x.Item2));

    private static void Validate(RecordStatus recordStatus, WorkflowStatus latestRevisionStatus)
    {
        if (!_validStates.Contains((recordStatus, latestRevisionStatus)))
        {
            throw new ArgumentException(
                $"A record with status [{recordStatus}] can't have a latest revision with status [{latestRevisionStatus}].",
                nameof(latestRevisionStatus)
            );
        }
    }
}
