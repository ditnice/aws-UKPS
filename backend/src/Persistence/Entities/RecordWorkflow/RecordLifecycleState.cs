using System.Runtime.InteropServices;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// The status of a record together with the workflow status of its latest revision.
/// </summary>
/// <param name="RecordStatus">The record's status.</param>
/// <param name="LatestRevisionStatus">The workflow status of the record's latest revision.</param>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct RecordLifecycleState(
    RecordStatus RecordStatus,
    WorkflowStatus LatestRevisionStatus
);
