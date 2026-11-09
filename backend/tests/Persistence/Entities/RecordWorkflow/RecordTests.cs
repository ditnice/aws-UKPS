using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using Command = UKPS.Api.Persistence.Entities.RecordWorkflow.RecordLifecycleStateMachine.Command;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class RecordTests
{
    private static readonly DateTime _now = new(2026, 3, 20, 15, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(
        RecordStatus.Unpublished,
        WorkflowStatus.Draft,
        true,
        new[] { RecordAction.Update, RecordAction.Discard, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Unpublished,
        WorkflowStatus.Draft,
        false,
        new[] { RecordAction.Update, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Unpublished,
        WorkflowStatus.QAReview,
        true,
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Unpublished,
        WorkflowStatus.Rejected,
        false,
        new[] { RecordAction.Update, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Published,
        WorkflowStatus.Draft,
        false,
        new[]
        {
            RecordAction.Update,
            RecordAction.ChangeStatus,
            RecordAction.ConfirmCurrent,
            RecordAction.ViewTimeline,
        }
    )]
    [InlineData(
        RecordStatus.Published,
        WorkflowStatus.QAReview,
        false,
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Published,
        WorkflowStatus.Published,
        true,
        new[]
        {
            RecordAction.Update,
            RecordAction.ChangeStatus,
            RecordAction.ConfirmCurrent,
            RecordAction.ViewTimeline,
        }
    )]
    [InlineData(
        RecordStatus.Published,
        WorkflowStatus.Rejected,
        false,
        new[]
        {
            RecordAction.Update,
            RecordAction.ChangeStatus,
            RecordAction.ConfirmCurrent,
            RecordAction.ViewTimeline,
        }
    )]
    [InlineData(
        RecordStatus.OnHold,
        WorkflowStatus.Draft,
        false,
        new[] { RecordAction.ChangeStatus, RecordAction.ConfirmOnHold, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.OnHold,
        WorkflowStatus.Published,
        false,
        new[] { RecordAction.ChangeStatus, RecordAction.ConfirmOnHold, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.OnHold,
        WorkflowStatus.Rejected,
        false,
        new[] { RecordAction.ChangeStatus, RecordAction.ConfirmOnHold, RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Archived,
        WorkflowStatus.Draft,
        false,
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Archived,
        WorkflowStatus.Published,
        false,
        new[] { RecordAction.ViewTimeline }
    )]
    [InlineData(
        RecordStatus.Archived,
        WorkflowStatus.Rejected,
        false,
        new[] { RecordAction.ViewTimeline }
    )]
    public void GetPermittedActions_ReturnsActionsForState(
        RecordStatus recordStatus,
        WorkflowStatus latestRevisionStatus,
        bool hasSingleRevision,
        RecordAction[] expected
    )
    {
        IReadOnlyCollection<RecordAction> actions = Record.GetPermittedActions(
            new(recordStatus, latestRevisionStatus),
            hasSingleRevision,
            canChangeStatus: true
        );

        actions.ShouldBe(expected);
    }

    [Theory]
    [InlineData(RecordStatus.Published, WorkflowStatus.Published)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Draft)]
    public void GetPermittedActions_WhenUserCannotChangeStatus_OmitsChangeStatus(
        RecordStatus recordStatus,
        WorkflowStatus latestRevisionStatus
    )
    {
        IReadOnlyCollection<RecordAction> actions = Record.GetPermittedActions(
            new(recordStatus, latestRevisionStatus),
            hasSingleRevision: false,
            canChangeStatus: false
        );

        actions.ShouldNotContain(RecordAction.ChangeStatus);
        actions.ShouldContain(RecordAction.ViewTimeline);
    }

    public static TheoryData<int, RecordStatus, WorkflowStatus, RecordStatus> PermittedStatusChanges
    {
        get
        {
            var data = new TheoryData<int, RecordStatus, WorkflowStatus, RecordStatus>();
            foreach (
                WorkflowStatus revisionStatus in new[]
                {
                    WorkflowStatus.Draft,
                    WorkflowStatus.Published,
                    WorkflowStatus.Rejected,
                }
            )
            {
                data.Add(
                    (int)Command.PutOnHold,
                    RecordStatus.Published,
                    revisionStatus,
                    RecordStatus.OnHold
                );
                data.Add(
                    (int)Command.Resume,
                    RecordStatus.OnHold,
                    revisionStatus,
                    RecordStatus.Published
                );
                data.Add(
                    (int)Command.Archive,
                    RecordStatus.Published,
                    revisionStatus,
                    RecordStatus.Archived
                );
                data.Add(
                    (int)Command.Archive,
                    RecordStatus.OnHold,
                    revisionStatus,
                    RecordStatus.Archived
                );
            }
            return data;
        }
    }

    // Command is internal, so the cases pass its value; xUnit test methods must be public.
    [Theory]
    [MemberData(nameof(PermittedStatusChanges))]
    public void TryChangeStatusByUser_WhenStatePermitsIt_ChangesStatusAndRecordsTheChange(
        int command,
        RecordStatus fromStatus,
        WorkflowStatus revisionStatus,
        RecordStatus expectedStatus
    )
    {
        User user = new UserFaker().Generate();
        (Record record, RecordRevision revision) = CreateRecord(fromStatus, revisionStatus);
        record.ReviewedAt = _now.AddMonths(-4);
        RecordStatusChangeReason reason = RecordStatusChangeReason.Other;

        bool isChanged = record.TryChangeStatusByUser(
            (Command)command,
            revision,
            _now,
            user,
            reason,
            "A note"
        );

        isChanged.ShouldBeTrue();
        record.RecordStatus.ShouldBe(expectedStatus);
        record.ReviewedAt.ShouldBe(_now);
        revision.WorkflowStatus.ShouldBe(revisionStatus);

        RecordStatusHistory history = record.StatusHistory.ShouldHaveSingleItem();
        history.FromStatus.ShouldBe(fromStatus);
        history.ToStatus.ShouldBe(expectedStatus);
        history.Reason.ShouldBe(reason);
        history.Note.ShouldBe("A note");
        history.UpdatedByUser.ShouldBe(user);
        history.UpdatedAt.ShouldBe(_now);

        RecordEvent recordEvent = record.Events.ShouldHaveSingleItem();
        recordEvent.EventType.ShouldBe(RecordEventType.RecordStatusChanged);
        recordEvent.Revision.ShouldBeNull();
        recordEvent.RevisionId.ShouldBeNull();
        recordEvent.PerformedByUser.ShouldBe(user);
        recordEvent.PerformedAt.ShouldBe(_now);
        recordEvent.Note.ShouldBeNull();
    }

    public static TheoryData<int, RecordStatus, WorkflowStatus> RejectedStatusChanges
    {
        get
        {
            var data = new TheoryData<int, RecordStatus, WorkflowStatus>();
            foreach (
                Command command in new[] { Command.PutOnHold, Command.Resume, Command.Archive }
            )
            {
                foreach (RecordLifecycleState state in RecordLifecycleState.ValidStates)
                {
                    if (!IsPermitted(command, state.RecordStatus, state.LatestRevisionStatus))
                    {
                        data.Add((int)command, state.RecordStatus, state.LatestRevisionStatus);
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(RejectedStatusChanges))]
    public void TryChangeStatusByUser_WhenStateDoesNotPermitIt_ChangesNothing(
        int command,
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus
    )
    {
        (Record record, RecordRevision revision) = CreateRecord(recordStatus, revisionStatus);
        DateTime reviewedAt = _now.AddMonths(-4);
        record.ReviewedAt = reviewedAt;

        bool isChanged = record.TryChangeStatusByUser(
            (Command)command,
            revision,
            _now,
            new UserFaker().Generate(),
            RecordStatusChangeReason.Other,
            "A note"
        );

        isChanged.ShouldBeFalse();
        record.ReviewedAt.ShouldBe(reviewedAt);
        ShouldBeUnchanged(record, revision, recordStatus, revisionStatus);
    }

    // The transitions table in documentation/records/status-management.md, restated
    // independently of the state machine.
    private static bool IsPermitted(
        Command command,
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus
    ) =>
        revisionStatus
            is WorkflowStatus.Draft
                or WorkflowStatus.Published
                or WorkflowStatus.Rejected
        && command switch
        {
            Command.PutOnHold => recordStatus == RecordStatus.Published,
            Command.Resume => recordStatus == RecordStatus.OnHold,
            Command.Archive => recordStatus is RecordStatus.Published or RecordStatus.OnHold,
            _ => false,
        };

    [Fact]
    public void TryDiscard_WhenOnlyRevisionIsDraft_DiscardsRecordAndRevision()
    {
        User user = new UserFaker().Generate();
        (Record record, RecordRevision revision) = CreateRecord(RecordStatus.Unpublished);

        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: true,
            _now,
            user,
            RecordStatusChangeReason.ManuallyDiscarded
        );

        isDiscarded.ShouldBeTrue();
        record.RecordStatus.ShouldBe(RecordStatus.Discarded);
        revision.WorkflowStatus.ShouldBe(WorkflowStatus.Discarded);

        RecordStatusHistory history = record.StatusHistory.ShouldHaveSingleItem();
        history.FromStatus.ShouldBe(RecordStatus.Unpublished);
        history.ToStatus.ShouldBe(RecordStatus.Discarded);
        history.Reason.ShouldBe(RecordStatusChangeReason.ManuallyDiscarded);
        history.UpdatedByUser.ShouldBe(user);
        history.UpdatedAt.ShouldBe(_now);

        RecordEvent recordEvent = record.Events.ShouldHaveSingleItem();
        recordEvent.EventType.ShouldBe(RecordEventType.RecordStatusChanged);
        recordEvent.Revision.ShouldBe(revision);
        recordEvent.PerformedByUser.ShouldBe(user);
        recordEvent.PerformedAt.ShouldBe(_now);
    }

    [Fact]
    public void TryDiscard_WhenRecordHasSeveralRevisions_ChangesNothing()
    {
        (Record record, RecordRevision revision) = CreateRecord(RecordStatus.Unpublished);

        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: false,
            _now,
            new UserFaker().Generate(),
            RecordStatusChangeReason.ManuallyDiscarded
        );

        isDiscarded.ShouldBeFalse();
        ShouldBeUnchanged(record, revision, RecordStatus.Unpublished, WorkflowStatus.Draft);
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.QAReview)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Draft)]
    public void TryDiscard_WhenStateDoesNotPermitDiscard_ChangesNothing(
        RecordStatus recordStatus,
        WorkflowStatus latestRevisionStatus
    )
    {
        (Record record, RecordRevision revision) = CreateRecord(recordStatus, latestRevisionStatus);

        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: true,
            _now,
            new UserFaker().Generate(),
            RecordStatusChangeReason.ManuallyDiscarded
        );

        isDiscarded.ShouldBeFalse();
        ShouldBeUnchanged(record, revision, recordStatus, latestRevisionStatus);
    }

    private static (Record Record, RecordRevision Revision) CreateRecord(
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus = WorkflowStatus.Draft
    )
    {
        Record record = new RecordFaker().RuleFor(x => x.RecordStatus, recordStatus).Generate();
        RecordRevision revision = new RecordRevisionFaker()
            .RuleFor(x => x.Record, record)
            .RuleFor(x => x.WorkflowStatus, revisionStatus)
            .Generate();
        return (record, revision);
    }

    private static void ShouldBeUnchanged(
        Record record,
        RecordRevision revision,
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus
    )
    {
        record.RecordStatus.ShouldBe(recordStatus);
        revision.WorkflowStatus.ShouldBe(revisionStatus);
        record.StatusHistory.ShouldBeEmpty();
        record.Events.ShouldBeEmpty();
    }
}
