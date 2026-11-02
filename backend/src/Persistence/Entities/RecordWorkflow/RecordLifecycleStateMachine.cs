using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.StateMachines;

namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// Governs the record status and the latest revision's workflow status together, since which
/// commands are permitted depends on both. Ignored commands are permitted but change neither
/// status.
/// </summary>
internal sealed class RecordLifecycleStateMachine
    : StateMachine<RecordLifecycleState, RecordLifecycleStateMachine.Command>
{
    public RecordLifecycleStateMachine(RecordLifecycleState initialState)
        : base(initialState)
    {
        DefineUnpublishedTransitions();
        DefinePublishedTransitions();
        DefineOnHoldTransitions();

        // Archived and discarded are terminal: no transitions are defined out of them, so any
        // command sent while in either is rejected. An archived record's draft stays draft.
    }

    private void DefineUnpublishedTransitions()
    {
        // Discard is only permitted on a record's first revision. The state can't show that,
        // so the caller must check it before sending the command.
        ForState(
            new(RecordStatus.Unpublished, WorkflowStatus.Draft),
            x =>
            {
                x.On(Command.Submit, new(RecordStatus.Unpublished, WorkflowStatus.QAReview));
                x.On(Command.Discard, new(RecordStatus.Discarded, WorkflowStatus.Discarded));
                x.Ignore(Command.EditDraft);
            }
        );

        ForState(
            new(RecordStatus.Unpublished, WorkflowStatus.QAReview),
            x =>
            {
                x.On(Command.Approve, new(RecordStatus.Published, WorkflowStatus.Published));
                x.On(Command.Reject, new(RecordStatus.Unpublished, WorkflowStatus.Rejected));
            }
        );

        ForState(
            new(RecordStatus.Unpublished, WorkflowStatus.Rejected),
            x => x.On(Command.CreateRevision, new(RecordStatus.Unpublished, WorkflowStatus.Draft))
        );
    }

    private void DefinePublishedTransitions()
    {
        ForState(
            new(RecordStatus.Published, WorkflowStatus.Draft),
            x =>
            {
                x.On(Command.Submit, new(RecordStatus.Published, WorkflowStatus.QAReview));
                x.On(Command.PutOnHold, new(RecordStatus.OnHold, WorkflowStatus.Draft));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Draft));
                x.Ignore(Command.EditDraft);
                x.Ignore(Command.ConfirmCurrent);
            }
        );

        // A revision in QA review locks the record until QA approves or rejects it.
        ForState(
            new(RecordStatus.Published, WorkflowStatus.QAReview),
            x =>
            {
                x.On(Command.Approve, new(RecordStatus.Published, WorkflowStatus.Published));
                x.On(Command.Reject, new(RecordStatus.Published, WorkflowStatus.Rejected));
            }
        );

        ForState(
            new(RecordStatus.Published, WorkflowStatus.Published),
            x =>
            {
                x.On(Command.CreateRevision, new(RecordStatus.Published, WorkflowStatus.Draft));
                x.On(Command.PutOnHold, new(RecordStatus.OnHold, WorkflowStatus.Published));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Published));
                x.Ignore(Command.ConfirmCurrent);
            }
        );

        ForState(
            new(RecordStatus.Published, WorkflowStatus.Rejected),
            x =>
            {
                x.On(Command.CreateRevision, new(RecordStatus.Published, WorkflowStatus.Draft));
                x.On(Command.PutOnHold, new(RecordStatus.OnHold, WorkflowStatus.Rejected));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Rejected));
                x.Ignore(Command.ConfirmCurrent);
            }
        );
    }

    // On hold locks the revisions: no revision can be created, edited or submitted until resumed.
    private void DefineOnHoldTransitions()
    {
        ForState(
            new(RecordStatus.OnHold, WorkflowStatus.Draft),
            x =>
            {
                x.On(Command.Resume, new(RecordStatus.Published, WorkflowStatus.Draft));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Draft));
                x.Ignore(Command.ConfirmCurrent);
            }
        );

        ForState(
            new(RecordStatus.OnHold, WorkflowStatus.Published),
            x =>
            {
                x.On(Command.Resume, new(RecordStatus.Published, WorkflowStatus.Published));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Published));
                x.Ignore(Command.ConfirmCurrent);
            }
        );

        ForState(
            new(RecordStatus.OnHold, WorkflowStatus.Rejected),
            x =>
            {
                x.On(Command.Resume, new(RecordStatus.Published, WorkflowStatus.Rejected));
                x.On(Command.Archive, new(RecordStatus.Archived, WorkflowStatus.Rejected));
                x.Ignore(Command.ConfirmCurrent);
            }
        );
    }

    internal enum Command
    {
        Submit = 0,
        Approve = 1,
        Reject = 2,
        CreateRevision = 3,
        PutOnHold = 4,
        Resume = 5,
        Archive = 6,
        Discard = 7,
        EditDraft = 8,
        ConfirmCurrent = 9,
    }
}
