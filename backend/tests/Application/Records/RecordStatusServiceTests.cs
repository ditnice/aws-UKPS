using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Configurations.RecordWorkflow;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordStatusServiceTests : DatabaseTestBase
{
    private static readonly DateTime _now = new(2026, 9, 1, 16, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _reviewedAt = new(2026, 6, 29, 9, 20, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private IServiceTestHarness<IRecordStatusService> _harness = null!;
    private IRecordStatusService Service => _harness.Service;

    public RecordStatusServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(731);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        _user = await AddEntity(new UserFaker().Generate(), TestContext.Current.CancellationToken);

        _harness = new ServiceTestHarness<IRecordStatusService>(Context)
            .UpdateCurrentTime(_now)
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = _organisation.Id,
                    UserRole = UserRole.Standard,
                }
            );
    }

    public static TheoryData<
        RecordStatusChangeAction,
        RecordStatus,
        WorkflowStatus[],
        RecordStatusChangeReason,
        RecordStatus
    > PermittedChanges =>
        new()
        {
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Published,
                [WorkflowStatus.Published],
                RecordStatusChangeReason.TrialSuspended,
                RecordStatus.OnHold
            },
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Published,
                [WorkflowStatus.Published, WorkflowStatus.Draft],
                RecordStatusChangeReason.FilingWithdrawn,
                RecordStatus.OnHold
            },
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Published,
                [WorkflowStatus.Published, WorkflowStatus.Rejected],
                RecordStatusChangeReason.Other,
                RecordStatus.OnHold
            },
            {
                RecordStatusChangeAction.Resume,
                RecordStatus.OnHold,
                [WorkflowStatus.Published],
                RecordStatusChangeReason.TrialResumed,
                RecordStatus.Published
            },
            {
                RecordStatusChangeAction.Resume,
                RecordStatus.OnHold,
                [WorkflowStatus.Published, WorkflowStatus.Draft],
                RecordStatusChangeReason.PlansRefiled,
                RecordStatus.Published
            },
            {
                RecordStatusChangeAction.Resume,
                RecordStatus.OnHold,
                [WorkflowStatus.Published, WorkflowStatus.Rejected],
                RecordStatusChangeReason.Other,
                RecordStatus.Published
            },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.Published,
                [WorkflowStatus.Published],
                RecordStatusChangeReason.DevelopmentDiscontinued,
                RecordStatus.Archived
            },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.Published,
                [WorkflowStatus.Published, WorkflowStatus.Draft],
                RecordStatusChangeReason.Other,
                RecordStatus.Archived
            },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.OnHold,
                [WorkflowStatus.Published, WorkflowStatus.Rejected],
                RecordStatusChangeReason.DevelopmentDiscontinued,
                RecordStatus.Archived
            },
        };

    [Theory]
    [MemberData(nameof(PermittedChanges))]
    public async Task ChangeStatus_WhenPermitted_ChangesStatusAndRecordsTheChange(
        RecordStatusChangeAction change,
        RecordStatus fromStatus,
        WorkflowStatus[] revisionStatuses,
        RecordStatusChangeReason reason,
        RecordStatus expectedStatus
    )
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddRecord(fromStatus, revisionStatuses);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand { Reason = reason, Note = "  Paused pending review  " },
            cancellationToken
        );

        result.IsOk.ShouldBeTrue();
        Record updated = await LoadRecord(record.Id, cancellationToken);
        updated.RecordStatus.ShouldBe(expectedStatus);
        updated.ReviewedAt.ShouldBe(_now);
        updated
            .Revisions.OrderBy(r => r.CreatedAt)
            .Select(r => r.WorkflowStatus)
            .ShouldBe(revisionStatuses);

        RecordStatusHistory history = updated.StatusHistory.ShouldHaveSingleItem();
        history.FromStatus.ShouldBe(fromStatus);
        history.ToStatus.ShouldBe(expectedStatus);
        history.Reason.ShouldBe(reason);
        history.Note.ShouldBe("Paused pending review");
        history.UpdatedBy.ShouldBe(_user.Id);
        history.UpdatedAt.ShouldBe(_now);

        RecordEvent recordEvent = updated.Events.ShouldHaveSingleItem();
        recordEvent.EventType.ShouldBe(RecordEventType.RecordStatusChanged);
        recordEvent.RevisionId.ShouldBeNull();
        recordEvent.PerformedBy.ShouldBe(_user.Id);
        recordEvent.PerformedAt.ShouldBe(_now);
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    public async Task ChangeStatus_WhenUserCanChangeStatus_ChangesIt(UserRole role)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);
        _harness.UpdateCurrentUser(x => x with { UserRole = role });

        Result<ChangeRecordStatusError> result = await PutOnHold(record.Id);

        result.IsOk.ShouldBeTrue();
        (await LoadRecord(record.Id, cancellationToken)).RecordStatus.ShouldBe(RecordStatus.OnHold);
    }

    [Fact]
    public async Task ChangeStatus_WhenNoteIsWhitespace_SavesNoNote()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);

        Result<ChangeRecordStatusError> result = await PutOnHold(record.Id, note: "   ");

        result.IsOk.ShouldBeTrue();
        (await LoadRecord(record.Id, cancellationToken))
            .StatusHistory.ShouldHaveSingleItem()
            .Note.ShouldBeNull();
    }

    [Fact]
    public async Task ChangeStatus_AfterChange_GetRecordReturnsNewStatusAndActions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);

        (await PutOnHold(record.Id)).IsOk.ShouldBeTrue();

        IRecordViewService viewService = new ServiceTestHarness<IRecordViewService>(Context)
            .UpdateCurrentUser(x =>
                x with
                {
                    OrganisationId = _organisation.Id,
                    UserRole = UserRole.Standard,
                }
            )
            .Service;
        RecordDto dto = (
            await viewService.GetRecord(
                record.Id,
                RecordType.Medicine,
                _organisation.Id,
                cancellationToken
            )
        ).ShouldBeSuccess();
        dto.RecordStatus.ShouldBe(RecordStatus.OnHold);
        dto.DisplayStatus.ShouldBe(RecordDisplayStatus.OnHold);
        dto.ReviewedAt.ShouldBe(_now);
        dto.Actions.ShouldBe([
            RecordAction.ChangeStatus,
            RecordAction.ConfirmOnHold,
            RecordAction.ViewTimeline,
        ]);
    }

    public static TheoryData<
        RecordStatusChangeAction,
        RecordStatus,
        WorkflowStatus[]
    > RejectedChanges =>
        new()
        {
            // Unpublished records can't use any of the endpoints.
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Unpublished,
                [WorkflowStatus.Draft]
            },
            { RecordStatusChangeAction.Resume, RecordStatus.Unpublished, [WorkflowStatus.Draft] },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.Unpublished,
                [WorkflowStatus.QAReview]
            },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.Unpublished,
                [WorkflowStatus.Rejected, WorkflowStatus.Draft]
            },
            // A revision in QA review locks the record.
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Published,
                [WorkflowStatus.Published, WorkflowStatus.QAReview]
            },
            {
                RecordStatusChangeAction.Archive,
                RecordStatus.Published,
                [WorkflowStatus.Published, WorkflowStatus.QAReview]
            },
            // Wrong direction.
            {
                RecordStatusChangeAction.Resume,
                RecordStatus.Published,
                [WorkflowStatus.Published]
            },
            { RecordStatusChangeAction.PutOnHold, RecordStatus.OnHold, [WorkflowStatus.Published] },
            // Archived is terminal.
            {
                RecordStatusChangeAction.PutOnHold,
                RecordStatus.Archived,
                [WorkflowStatus.Published]
            },
            { RecordStatusChangeAction.Resume, RecordStatus.Archived, [WorkflowStatus.Published] },
            { RecordStatusChangeAction.Archive, RecordStatus.Archived, [WorkflowStatus.Draft] },
        };

    [Theory]
    [MemberData(nameof(RejectedChanges))]
    public async Task ChangeStatus_WhenStateDoesNotPermitIt_ReturnsTransitionNotPermitted(
        RecordStatusChangeAction change,
        RecordStatus recordStatus,
        WorkflowStatus[] revisionStatuses
    )
    {
        Record record = await AddRecord(recordStatus, revisionStatuses);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand
            {
                Reason = RecordStatusChangeReason.Other,
                Note = "Detail",
            },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.TransitionNotPermitted>();
        await ShouldBeUnchanged(record.Id, recordStatus);
    }

    [Theory]
    [InlineData(RecordStatusChangeAction.PutOnHold)]
    [InlineData(RecordStatusChangeAction.Resume)]
    [InlineData(RecordStatusChangeAction.Archive)]
    public async Task ChangeStatus_WhenRecordIsDiscarded_ReturnsNotFound(
        RecordStatusChangeAction change
    )
    {
        Record record = await AddRecord(RecordStatus.Discarded, [WorkflowStatus.Discarded]);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand
            {
                Reason = RecordStatusChangeReason.Other,
                Note = "Detail",
            },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotFound>();
        Record unchanged = await Context
            .Records.IgnoreQueryFilters([RecordConfiguration.DiscardedFilter])
            .AsNoTracking()
            .Include(r => r.StatusHistory)
            .SingleAsync(r => r.Id == record.Id, TestContext.Current.CancellationToken);
        unchanged.RecordStatus.ShouldBe(RecordStatus.Discarded);
        unchanged.StatusHistory.ShouldBeEmpty();
    }

    // The reasons each change accepts, restated independently of the service.
    private static readonly Dictionary<
        RecordStatusChangeAction,
        RecordStatusChangeReason[]
    > _validReasons = new()
    {
        [RecordStatusChangeAction.PutOnHold] =
        [
            RecordStatusChangeReason.TrialSuspended,
            RecordStatusChangeReason.FilingWithdrawn,
            RecordStatusChangeReason.Other,
        ],
        [RecordStatusChangeAction.Resume] =
        [
            RecordStatusChangeReason.TrialResumed,
            RecordStatusChangeReason.PlansRefiled,
            RecordStatusChangeReason.Other,
        ],
        [RecordStatusChangeAction.Archive] =
        [
            RecordStatusChangeReason.DevelopmentDiscontinued,
            RecordStatusChangeReason.Other,
        ],
    };

    public static TheoryData<RecordStatusChangeAction, RecordStatusChangeReason?> InvalidReasons
    {
        get
        {
            var data = new TheoryData<RecordStatusChangeAction, RecordStatusChangeReason?>();
            foreach (
                (RecordStatusChangeAction change, RecordStatusChangeReason[] valid) in _validReasons
            )
            {
                data.Add(change, null);
                foreach (
                    RecordStatusChangeReason reason in Enum.GetValues<RecordStatusChangeReason>()
                        .Except(valid)
                )
                {
                    data.Add(change, reason);
                }
            }
            return data;
        }
    }

    public static TheoryData<RecordStatusChangeAction, RecordStatusChangeReason> ValidReasons
    {
        get
        {
            var data = new TheoryData<RecordStatusChangeAction, RecordStatusChangeReason>();
            foreach (
                (RecordStatusChangeAction change, RecordStatusChangeReason[] valid) in _validReasons
            )
            {
                foreach (RecordStatusChangeReason reason in valid)
                {
                    data.Add(change, reason);
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ValidReasons))]
    public async Task ChangeStatus_WhenReasonIsValidForTheChange_ChangesStatus(
        RecordStatusChangeAction change,
        RecordStatusChangeReason reason
    )
    {
        RecordStatus status =
            change == RecordStatusChangeAction.Resume
                ? RecordStatus.OnHold
                : RecordStatus.Published;
        Record record = await AddRecord(status, [WorkflowStatus.Published]);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand { Reason = reason, Note = "Detail" },
            TestContext.Current.CancellationToken
        );

        result.IsOk.ShouldBeTrue();
        (await LoadRecord(record.Id, TestContext.Current.CancellationToken))
            .StatusHistory.ShouldHaveSingleItem()
            .Reason.ShouldBe(reason);
    }

    [Theory]
    [MemberData(nameof(InvalidReasons))]
    public async Task ChangeStatus_WhenReasonIsNotValidForTheChange_ReturnsInvalidReason(
        RecordStatusChangeAction change,
        RecordStatusChangeReason? reason
    )
    {
        // Published and on hold together cover every change's starting state.
        RecordStatus status =
            change == RecordStatusChangeAction.Resume
                ? RecordStatus.OnHold
                : RecordStatus.Published;
        Record record = await AddRecord(status, [WorkflowStatus.Published]);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand { Reason = reason },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.InvalidReason>();
        await ShouldBeUnchanged(record.Id, status);
    }

    [Theory]
    [InlineData(RecordStatusChangeAction.PutOnHold, null)]
    [InlineData(RecordStatusChangeAction.Resume, "")]
    [InlineData(RecordStatusChangeAction.Archive, "   ")]
    public async Task ChangeStatus_WhenReasonIsOtherWithoutANote_ReturnsNoteRequired(
        RecordStatusChangeAction change,
        string? note
    )
    {
        RecordStatus status =
            change == RecordStatusChangeAction.Resume
                ? RecordStatus.OnHold
                : RecordStatus.Published;
        Record record = await AddRecord(status, [WorkflowStatus.Published]);

        Result<ChangeRecordStatusError> result = await Service.ChangeStatus(
            record.Id,
            _organisation.Id,
            change,
            new ChangeRecordStatusCommand { Reason = RecordStatusChangeReason.Other, Note = note },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NoteRequired>();
        await ShouldBeUnchanged(record.Id, status);
    }

    [Fact]
    public async Task ChangeStatus_WhenRecordDoesNotExist_ReturnsNotFound()
    {
        Result<ChangeRecordStatusError> result = await PutOnHold(int.MaxValue);

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotFound>();
    }

    [Fact]
    public async Task ChangeStatus_WhenRecordIsInAnotherOrganisation_ReturnsNotFound()
    {
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        Record record = await AddRecord(
            RecordStatus.Published,
            [WorkflowStatus.Published],
            otherOrganisation.Id
        );

        // A guessed ID from another organisation, requested as the caller's own.
        Result<ChangeRecordStatusError> result = await PutOnHold(record.Id);

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotFound>();
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenCallerCannotReadTheOrganisation_ReturnsNotAllowed()
    {
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        Record record = await AddRecord(
            RecordStatus.Published,
            [WorkflowStatus.Published],
            otherOrganisation.Id
        );

        Result<ChangeRecordStatusError> result = await PutOnHold(
            record.Id,
            organisationId: otherOrganisation.Id
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotAllowed>();
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenCallerCannotReadTheOrganisation_ReturnsNotAllowedForMissingRecord()
    {
        Organisation otherOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );

        Result<ChangeRecordStatusError> result = await PutOnHold(
            int.MaxValue,
            organisationId: otherOrganisation.Id
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotAllowed>();
    }

    [Fact]
    public async Task ChangeStatus_WhenCallerCanReadButNotChangeStatus_ReturnsCannotChangeStatus()
    {
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);
        _harness.UpdateCurrentUser(x => x with { UserRole = UserRole.Super });

        Result<ChangeRecordStatusError> result = await PutOnHold(record.Id);

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.CannotChangeStatus>();
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenCallerCanReadButNotChangeStatus_ReturnsNotFoundForMissingRecord()
    {
        _harness.UpdateCurrentUser(x => x with { UserRole = UserRole.Super });

        Result<ChangeRecordStatusError> result = await PutOnHold(int.MaxValue);

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.NotFound>();
    }

    [Fact]
    public async Task ChangeStatus_WhenOrganisationIsSelectableButNotSelected_ThrowsMismatch()
    {
        Organisation selectedOrganisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);
        _harness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = selectedOrganisation.Id,
                SelectableOrganisationIds = [selectedOrganisation.Id, _organisation.Id],
            }
        );

        SelectedOrganisationMismatchException exception =
            await Should.ThrowAsync<SelectedOrganisationMismatchException>(() =>
                PutOnHold(record.Id)
            );

        exception.OrganisationId.ShouldBe(_organisation.Id);
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenRevisionIsSubmittedBeforeSaving_ReturnsConcurrencyError()
    {
        Record record = await AddRecord(
            RecordStatus.Published,
            [WorkflowStatus.Published, WorkflowStatus.Draft]
        );

        Result<ChangeRecordStatusError> result = await PutOnHoldWhileTimeIsRead(
            record.Id,
            revisions =>
                revisions
                    .Where(r => r.WorkflowStatus == WorkflowStatus.Draft)
                    .ExecuteUpdate(s =>
                        s.SetProperty(r => r.WorkflowStatus, WorkflowStatus.QAReview)
                    )
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.ConcurrencyError>();
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenContentIsSavedBeforeSaving_ReturnsConcurrencyError()
    {
        Record record = await AddRecord(
            RecordStatus.Published,
            [WorkflowStatus.Published, WorkflowStatus.Draft]
        );

        // A content save rewrites the revision's status unchanged, moving its version on.
        Result<ChangeRecordStatusError> result = await PutOnHoldWhileTimeIsRead(
            record.Id,
            revisions =>
                revisions
                    .Where(r => r.WorkflowStatus == WorkflowStatus.Draft)
                    .ExecuteUpdate(s => s.SetProperty(r => r.WorkflowStatus, r => r.WorkflowStatus))
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.ConcurrencyError>();
        await ShouldBeUnchanged(record.Id, RecordStatus.Published);
    }

    [Fact]
    public async Task ChangeStatus_WhenAnotherStatusChangeLandsFirst_ReturnsConcurrencyError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Published, [WorkflowStatus.Published]);

        // Another user archives the record after this change has checked it.
        await using AppDbContext otherContext = Fixture.CreateContext();
        IRecordStatusService otherService = new ServiceTestHarness<IRecordStatusService>(
            otherContext
        )
            .UpdateCurrentTime(_now)
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = _organisation.Id,
                    UserRole = UserRole.Standard,
                }
            )
            .Service;
        IRecordStatusService service = _harness
            .ConfigureServices(services =>
                services.AddSingleton<IDateTimeProvider>(
                    new ChangeWhenTimeIsReadProvider(
                        () =>
                            otherService
                                .ChangeStatus(
                                    record.Id,
                                    _organisation.Id,
                                    RecordStatusChangeAction.Archive,
                                    new ChangeRecordStatusCommand
                                    {
                                        Reason = RecordStatusChangeReason.DevelopmentDiscontinued,
                                    },
                                    cancellationToken
                                )
                                .GetAwaiter()
                                .GetResult()
                                .IsOk.ShouldBeTrue(),
                        _now
                    )
                )
            )
            .Service;

        Result<ChangeRecordStatusError> result = await service.ChangeStatus(
            record.Id,
            _organisation.Id,
            RecordStatusChangeAction.PutOnHold,
            new ChangeRecordStatusCommand { Reason = RecordStatusChangeReason.TrialSuspended },
            cancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<ChangeRecordStatusError.ConcurrencyError>();
        Record updated = await LoadRecord(record.Id, cancellationToken);
        updated.RecordStatus.ShouldBe(RecordStatus.Archived);
        updated.StatusHistory.ShouldHaveSingleItem().ToStatus.ShouldBe(RecordStatus.Archived);
        updated.Events.ShouldHaveSingleItem();
    }

    private Task<Result<ChangeRecordStatusError>> PutOnHold(
        int recordId,
        string? note = null,
        int? organisationId = null
    ) =>
        Service.ChangeStatus(
            recordId,
            organisationId ?? _organisation.Id,
            RecordStatusChangeAction.PutOnHold,
            new ChangeRecordStatusCommand
            {
                Reason = RecordStatusChangeReason.TrialSuspended,
                Note = note,
            },
            TestContext.Current.CancellationToken
        );

    // The service reads the time after checking the record and before saving, so the other
    // change is made then.
    private async Task<Result<ChangeRecordStatusError>> PutOnHoldWhileTimeIsRead(
        int recordId,
        Func<IQueryable<RecordRevision>, int> change
    )
    {
        await using AppDbContext otherContext = Fixture.CreateContext();
        IRecordStatusService service = _harness
            .ConfigureServices(services =>
                services.AddSingleton<IDateTimeProvider>(
                    new ChangeWhenTimeIsReadProvider(
                        () =>
                            change(otherContext.RecordRevisions.Where(r => r.RecordId == recordId))
                                .ShouldBe(1),
                        _now
                    )
                )
            )
            .Service;

        return await service.ChangeStatus(
            recordId,
            _organisation.Id,
            RecordStatusChangeAction.PutOnHold,
            new ChangeRecordStatusCommand { Reason = RecordStatusChangeReason.TrialSuspended },
            TestContext.Current.CancellationToken
        );
    }

    private async Task<Record> LoadRecord(int recordId, CancellationToken cancellationToken)
    {
        Context.ChangeTracker.Clear();
        return await Context
            .Records.AsNoTracking()
            .Include(r => r.Revisions)
            .Include(r => r.StatusHistory)
            .Include(r => r.Events)
            .SingleAsync(r => r.Id == recordId, cancellationToken);
    }

    private async Task ShouldBeUnchanged(int recordId, RecordStatus expectedStatus)
    {
        Record record = await LoadRecord(recordId, TestContext.Current.CancellationToken);
        record.RecordStatus.ShouldBe(expectedStatus);
        record.ReviewedAt.ShouldBe(_reviewedAt);
        record.StatusHistory.ShouldBeEmpty();
        record.Events.ShouldBeEmpty();
    }

    private async Task<Record> AddRecord(
        RecordStatus status,
        WorkflowStatus[] revisionStatuses,
        int? organisationId = null
    )
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Record record = await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, organisationId ?? _organisation.Id)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, status)
                .RuleFor(x => x.ReviewedAt, _reviewedAt)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .Generate(),
            cancellationToken
        );
        await AddEntities(
            revisionStatuses
                .Select(
                    (workflowStatus, i) =>
                        new RecordRevisionFaker()
                            .RuleFor(x => x.RecordId, record.Id)
                            .RuleFor(x => x.CreatedBy, _user.Id)
                            .RuleFor(x => x.WorkflowStatus, workflowStatus)
                            .RuleFor(x => x.CreatedAt, _reviewedAt.AddDays(i - 30))
                            .Generate()
                )
                .ToArray(),
            cancellationToken
        );
        Context.ChangeTracker.Clear();
        return record;
    }

    private sealed class ChangeWhenTimeIsReadProvider(Action change, DateTime now)
        : IDateTimeProvider
    {
        public DateTime GetUtcNow()
        {
            change();
            return now;
        }

        public DateTimeOffset GetOffsetUtcNow() => new(GetUtcNow());
    }
}
