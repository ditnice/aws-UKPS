using Shouldly;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class RecordLifecycleStateTests
{
    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.Published)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.QAReview)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.QAReview)]
    [InlineData(RecordStatus.Discarded, WorkflowStatus.Draft)]
    [InlineData(RecordStatus.Published, WorkflowStatus.Discarded)]
    public void Constructor_ShouldThrow_WhenStatusesCannotCoexist(
        RecordStatus recordStatus,
        WorkflowStatus latestRevisionStatus
    )
    {
        Should.Throw<ArgumentException>(() =>
            new RecordLifecycleState(recordStatus, latestRevisionStatus)
        );
    }
}
