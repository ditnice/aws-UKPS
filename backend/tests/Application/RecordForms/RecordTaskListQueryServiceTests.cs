using Bogus;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>The task list query service against a real database.</summary>
[Collection(DatabaseCollection.Name)]
public class RecordTaskListQueryServiceTests : DatabaseTestBase
{
    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;
    private BnfChapter _bnfChapter = null!;
    private TherapeuticArea _area = null!;

    public RecordTaskListQueryServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(7);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(new OrganisationFaker().Generate(), Ct);
        _user = await AddEntity(new UserFaker().Generate(), Ct);
        _record = await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, _organisation.Id)
                .RuleFor(x => x.RecordType, RecordType.Medicine)
                .RuleFor(x => x.RecordStatus, RecordStatus.Unpublished)
                .Generate(),
            Ct
        );
        _revision = await AddRevision(WorkflowStatus.Draft, DateTime.UtcNow);
        _bnfChapter = await AddEntity(
            new BnfChapter
            {
                Code = "5",
                Label = "Infections",
                DisplayOrder = 1,
            },
            Ct
        );
        _area = await AddEntity(
            new TherapeuticArea
            {
                Code = "10",
                Label = "Viral infectious disease",
                DisplayOrder = 1,
            },
            Ct
        );
    }

    [Fact]
    public async Task GetTaskList_NoSavedAnswers_ListsTheFormWithTheTaskNotStarted()
    {
        RecordTaskListDto taskList = await GetTaskList();

        taskList.RecordId.ShouldBe(_record.Id);
        taskList.OrganisationId.ShouldBe(_organisation.Id);
        RecordTaskListSectionDto section = taskList.Sections.ShouldHaveSingleItem();
        (section.Id, section.Title).ShouldBe(("indication-details", "Indication details"));
        RecordTaskListTaskDto task = section.Tasks.ShouldHaveSingleItem();
        (task.Id, task.Title, task.FirstPageId).ShouldBe(
            ("indication-details", "Indication details", "indication")
        );
        task.Status.ShouldBe(RecordTaskStatus.NotStarted);
    }

    [Fact]
    public async Task GetTaskList_RowWithOnlyBlankAnswers_IsNotStarted()
    {
        await AddIndicationRow(new MedicinesIndicationDetail { Indication = "   " });

        (await GetStatus()).ShouldBe(RecordTaskStatus.NotStarted);
    }

    [Fact]
    public async Task GetTaskList_SomeRequiredAnswers_IsInProgress()
    {
        await AddIndicationRow(
            new MedicinesIndicationDetail
            {
                Indication = "Hepatitis C",
                BnfChapterId = _bnfChapter.Id,
            }
        );

        (await GetStatus()).ShouldBe(RecordTaskStatus.InProgress);
    }

    [Fact]
    public async Task GetTaskList_OnlyAnOptionalJunctionAnswer_IsInProgress()
    {
        await AddIndicationRow(
            new MedicinesIndicationDetail
            {
                TherapeuticAreas =
                [
                    new MedicinesIndicationDetailTherapeuticArea { TherapeuticAreaId = _area.Id },
                ],
            }
        );

        (await GetStatus()).ShouldBe(RecordTaskStatus.InProgress);
    }

    [Fact]
    public async Task GetTaskList_EveryRequiredAnswerWithoutTheOptionalOne_IsCompleted()
    {
        await AddIndicationRow(CompleteRow());

        (await GetStatus()).ShouldBe(RecordTaskStatus.Completed);
    }

    [Fact]
    public async Task GetTaskList_AnsweredEarlierRevision_ReadsOnlyTheLatestRevision()
    {
        RecordRevision earlier = await AddRevision(
            WorkflowStatus.Published,
            _revision.CreatedAt.AddDays(-1)
        );
        MedicinesIndicationDetail row = CompleteRow();
        row.RevisionId = earlier.Id;
        await AddEntity(row, Ct);

        (await GetStatus()).ShouldBe(RecordTaskStatus.NotStarted);
    }

    [Fact]
    public async Task GetTaskList_UnknownRecord_IsNotFound()
    {
        (await CreateHarness(_organisation.Id).Service.GetTaskList(_record.Id + 1, Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordTaskListError.NotFound>();
    }

    [Fact]
    public async Task GetTaskList_AnotherOrganisationsRecord_IsNotAllowed()
    {
        (await CreateHarness(_organisation.Id + 100).Service.GetTaskList(_record.Id, Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordTaskListError.NotAllowed>();
    }

    private MedicinesIndicationDetail CompleteRow() =>
        new()
        {
            Indication = "Hepatitis C",
            BnfChapterId = _bnfChapter.Id,
            IndicationIsPaediatric = IndicationPaediatricStatus.ExclusivelyAdults,
            IndicationIsCancer = YesNoUnknown.No,
            IndicationIsRareDisease = YesNoUnknown.Unknown,
        };

    private async Task AddIndicationRow(MedicinesIndicationDetail row)
    {
        row.RevisionId = _revision.Id;
        await AddEntity(row, Ct);
    }

    private async Task<RecordRevision> AddRevision(WorkflowStatus status, DateTime createdAt) =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, status)
                .RuleFor(x => x.CreatedAt, createdAt)
                .Generate(),
            Ct
        );

    private async Task<RecordTaskListDto> GetTaskList() =>
        (
            await CreateHarness(_organisation.Id).Service.GetTaskList(_record.Id, Ct)
        ).ShouldBeSuccess();

    private async Task<RecordTaskStatus> GetStatus() =>
        (await GetTaskList()).Sections.Single().Tasks.Single().Status;

    private ServiceTestHarness<IRecordTaskListQueryService> CreateHarness(int organisationId)
    {
        ServiceTestHarness<IRecordTaskListQueryService> harness = new(Context);
        harness.UpdateCurrentUser(x =>
            x with
            {
                CognitoUsername = _user.CognitoUsername,
                OrganisationId = organisationId,
                UserRole = UserRole.Standard,
            }
        );
        return harness;
    }
}
