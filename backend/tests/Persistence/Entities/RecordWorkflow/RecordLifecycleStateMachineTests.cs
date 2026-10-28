using Shouldly;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using Command = UKPS.Api.Persistence.Entities.RecordWorkflow.RecordLifecycleStateMachine.Command;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class RecordLifecycleStateMachineTests
{
    private static readonly RecordLifecycleState NewRecord = new(
        RecordStatus.Unpublished,
        WorkflowStatus.Draft
    );

    /// <summary>
    /// The combinations of record status and latest revision status that can coexist.
    /// </summary>
    private static readonly RecordLifecycleState[] AllowedStates =
    [
        new(RecordStatus.Unpublished, WorkflowStatus.Draft),
        new(RecordStatus.Published, WorkflowStatus.Draft),
        new(RecordStatus.OnHold, WorkflowStatus.Draft),
        new(RecordStatus.Archived, WorkflowStatus.Draft),
        new(RecordStatus.Published, WorkflowStatus.Published),
        new(RecordStatus.OnHold, WorkflowStatus.Published),
        new(RecordStatus.Archived, WorkflowStatus.Published),
        new(RecordStatus.Unpublished, WorkflowStatus.QAReview),
        new(RecordStatus.Published, WorkflowStatus.QAReview),
        new(RecordStatus.Unpublished, WorkflowStatus.Rejected),
        new(RecordStatus.Published, WorkflowStatus.Rejected),
        new(RecordStatus.OnHold, WorkflowStatus.Rejected),
        new(RecordStatus.Archived, WorkflowStatus.Rejected),
        new(RecordStatus.Discarded, WorkflowStatus.Discarded),
    ];

    private static readonly Dictionary<
        (RecordLifecycleState From, Command Command),
        RecordLifecycleState
    > ExpectedTransitions = new()
    {
        [(new(RecordStatus.Unpublished, WorkflowStatus.Draft), Command.Submit)] = new(
            RecordStatus.Unpublished,
            WorkflowStatus.QAReview
        ),
        [(new(RecordStatus.Unpublished, WorkflowStatus.Draft), Command.Discard)] = new(
            RecordStatus.Discarded,
            WorkflowStatus.Discarded
        ),
        [(new(RecordStatus.Unpublished, WorkflowStatus.QAReview), Command.Approve)] = new(
            RecordStatus.Published,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.Unpublished, WorkflowStatus.QAReview), Command.Reject)] = new(
            RecordStatus.Unpublished,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.Unpublished, WorkflowStatus.Rejected), Command.CreateRevision)] = new(
            RecordStatus.Unpublished,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Draft), Command.Submit)] = new(
            RecordStatus.Published,
            WorkflowStatus.QAReview
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Draft), Command.PutOnHold)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Draft), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.QAReview), Command.Approve)] = new(
            RecordStatus.Published,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.Published, WorkflowStatus.QAReview), Command.Reject)] = new(
            RecordStatus.Published,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Published), Command.CreateRevision)] = new(
            RecordStatus.Published,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Published), Command.PutOnHold)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Published), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Rejected), Command.CreateRevision)] = new(
            RecordStatus.Published,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Rejected), Command.PutOnHold)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Rejected), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Draft), Command.Resume)] = new(
            RecordStatus.Published,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Draft), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Published), Command.Resume)] = new(
            RecordStatus.Published,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Published), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Rejected), Command.Resume)] = new(
            RecordStatus.Published,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Rejected), Command.Archive)] = new(
            RecordStatus.Archived,
            WorkflowStatus.Rejected
        ),

        // Permitted without changing either status.
        [(new(RecordStatus.Unpublished, WorkflowStatus.Draft), Command.EditDraft)] = new(
            RecordStatus.Unpublished,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Draft), Command.EditDraft)] = new(
            RecordStatus.Published,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Draft), Command.ConfirmCurrent)] = new(
            RecordStatus.Published,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Published), Command.ConfirmCurrent)] = new(
            RecordStatus.Published,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.Published, WorkflowStatus.Rejected), Command.ConfirmCurrent)] = new(
            RecordStatus.Published,
            WorkflowStatus.Rejected
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Draft), Command.ConfirmCurrent)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Draft
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Published), Command.ConfirmCurrent)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Published
        ),
        [(new(RecordStatus.OnHold, WorkflowStatus.Rejected), Command.ConfirmCurrent)] = new(
            RecordStatus.OnHold,
            WorkflowStatus.Rejected
        ),
    };

    [Fact]
    public void TrySendCommand_ShouldOnlyPermitExpectedTransitions_FromEveryAllowedState()
    {
        List<string> mismatches = [];
        foreach (RecordLifecycleState from in AllowedStates)
        {
            foreach (Command command in Enum.GetValues<Command>())
            {
                var stateMachine = new RecordLifecycleStateMachine(from);

                var result = stateMachine.TrySendCommand(command);

                bool isExpected = ExpectedTransitions.TryGetValue(
                    (from, command),
                    out RecordLifecycleState expectedNext
                );
                RecordLifecycleState expectedCurrent = isExpected ? expectedNext : from;
                if (result.Success != isExpected || result.CurrentState != expectedCurrent)
                {
                    mismatches.Add(
                        $"{from} + {command}: expected success={isExpected} to {expectedCurrent}, "
                            + $"got success={result.Success} to {result.CurrentState}"
                    );
                }
            }
        }

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void TrySendCommand_ShouldOnlyReachAllowedStates_FromNewRecord()
    {
        HashSet<RecordLifecycleState> reached = [NewRecord];
        Queue<RecordLifecycleState> toVisit = new([NewRecord]);
        while (toVisit.TryDequeue(out RecordLifecycleState from))
        {
            foreach (Command command in Enum.GetValues<Command>())
            {
                var result = new RecordLifecycleStateMachine(from).TrySendCommand(command);
                if (result.Success && reached.Add(result.CurrentState))
                {
                    toVisit.Enqueue(result.CurrentState);
                }
            }
        }

        reached.ShouldBe(AllowedStates, ignoreOrder: true);
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished)]
    [InlineData(RecordStatus.Published)]
    public void TrySendCommand_ShouldRejectHoldAndArchive_WhenLatestRevisionInQaReview(
        RecordStatus recordStatus
    )
    {
        RecordLifecycleState inQaReview = new(recordStatus, WorkflowStatus.QAReview);

        new RecordLifecycleStateMachine(inQaReview)
            .TrySendCommand(Command.PutOnHold)
            .Success.ShouldBeFalse();
        new RecordLifecycleStateMachine(inQaReview)
            .TrySendCommand(Command.Archive)
            .Success.ShouldBeFalse();
    }

    [Theory]
    [InlineData(WorkflowStatus.Draft)]
    [InlineData(WorkflowStatus.Published)]
    [InlineData(WorkflowStatus.Rejected)]
    public void TrySendCommand_ShouldRejectRevisionCommands_WhenOnHold(
        WorkflowStatus latestRevisionStatus
    )
    {
        RecordLifecycleState onHold = new(RecordStatus.OnHold, latestRevisionStatus);

        foreach (
            Command command in new[] { Command.CreateRevision, Command.EditDraft, Command.Submit }
        )
        {
            new RecordLifecycleStateMachine(onHold).TrySendCommand(command).Success.ShouldBeFalse();
        }
    }

    [Fact]
    public void TrySendCommand_ShouldKeepDraftRevision_WhenArchived()
    {
        var stateMachine = new RecordLifecycleStateMachine(
            new(RecordStatus.Published, WorkflowStatus.Draft)
        );

        var result = stateMachine.TrySendCommand(Command.Archive);

        result.Success.ShouldBeTrue();
        result.CurrentState.ShouldBe(new(RecordStatus.Archived, WorkflowStatus.Draft));
        result.PermittedNextState.ShouldBeEmpty();
    }
}
