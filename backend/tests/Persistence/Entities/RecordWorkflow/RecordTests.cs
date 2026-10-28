using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class RecordTests
{
    private static readonly DateTime Now = new(2026, 3, 20, 15, 0, 0, DateTimeKind.Utc);

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
            hasSingleRevision
        );

        actions.ShouldBe(expected);
    }

    [Fact]
    public void TryDiscard_WhenOnlyRevisionIsDraft_DiscardsRecordAndRevision()
    {
        User user = new UserFaker().Generate();
        (Record record, RecordRevision revision) = CreateRecord(RecordStatus.Unpublished);

        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: true,
            Now,
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
        history.UpdatedAt.ShouldBe(Now);

        RecordEvent recordEvent = record.Events.ShouldHaveSingleItem();
        recordEvent.EventType.ShouldBe(RecordEventType.RecordStatusChanged);
        recordEvent.Revision.ShouldBe(revision);
        recordEvent.PerformedByUser.ShouldBe(user);
        recordEvent.PerformedAt.ShouldBe(Now);
    }

    [Fact]
    public void TryDiscard_WhenRecordHasSeveralRevisions_ChangesNothing()
    {
        (Record record, RecordRevision revision) = CreateRecord(RecordStatus.Unpublished);

        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: false,
            Now,
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
            Now,
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
