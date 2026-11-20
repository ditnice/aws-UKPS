using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;
using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.SaveRecordPageError
>;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// The clinical trials task against a real database: a yes or no gate, then a group of trials
/// each with a nested list of other registry numbers, stored as child rows.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RecordFormClinicalTrialsServiceTests : DatabaseTestBase
{
    private const string HasTrials = "record_clinical_trial_information.has_clinical_trials";
    private const string Trials = "record_clinical_trial";
    private const string Recruiting = "record_clinical_trial_information.recruiting_in_uk";
    private const string GatePage = "add-clinical-trial";
    private const string DetailsPage = "clinical-trial-details";
    private const string RecruitingPage = "recruiting-in-uk";

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private ServiceTestHarness<IRecordFormQueryService> _queryHarness = null!;
    private ServiceTestHarness<IRecordFormSaveService> _saveHarness = null!;
    private ServiceTestHarness<IRecordTaskListQueryService> _taskListHarness = null!;

    public RecordFormClinicalTrialsServiceTests(PostgresFixture fixture)
        : base(fixture) { }

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
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
                .Generate(),
            Ct
        );

        _queryHarness = CreateHarness<IRecordFormQueryService>();
        _saveHarness = CreateHarness<IRecordFormSaveService>();
        _taskListHarness = CreateHarness<IRecordTaskListQueryService>();
    }

    [Fact]
    public async Task GetPage_Gate_OffersYesAndNoAndMayLeadToTheDetails()
    {
        RecordPageDto page = await GetPage(GatePage);

        page.Questions.Single().Options!.Select(static o => o.Label).ShouldBe(["Yes", "No"]);
        page.NextPageId.ShouldBe(DetailsPage);
        (await _queryHarness.Service.GetPage(_record.Id, DetailsPage, Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();
    }

    [Fact]
    public async Task SavePage_No_ReturnsToTheTaskListWithTheTaskComplete()
    {
        (await SaveOk(GatePage, Answer(HasTrials, "No"))).NextPageId.ShouldBeNull();

        (await TaskStatus()).ShouldBe(RecordTaskStatus.Completed);
    }

    [Fact]
    public async Task GetPage_Details_DescribesTheGroupAndStartsEmpty()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        RecordPageDto page = await GetPage(DetailsPage);

        RecordPageQuestionDto group = page.Questions.Single();
        (group.Type, group.Multiple, group.ItemLabel, group.RemoveLabel).ShouldBe(
            (FormControl.Group, false, "Clinical trial", "Remove clinical trial")
        );
        group
            .Children!.Select(static c => (c.Id, c.Type, c.Multiple))
            .ShouldBe([
                ("study_name", FormControl.Text, false),
                ("clinical_trials_gov_number", FormControl.PrefixedNumber, false),
                ("other_registry_numbers", FormControl.TextList, true),
            ]);
        group.Children![1].Prefixes.ShouldBe(["NCT"]);
        group.Children[1].PrefixDigits.ShouldBe(8);
        page.Answers[Trials].GetRawText().ShouldBe("[]");
        page.NextPageId.ShouldBe(RecruitingPage);
    }

    [Fact]
    public async Task SavePage_Trials_StoresEachTrialAndItsRegistryNumbersInOrder()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        SaveRecordPageDto saved = await SaveOk(
            DetailsPage,
            TrialsAnswer(
                Trial("ION-UK", "NCT06123456", "2025-001234-56", "ISRCTN12345678"),
                Trial("ION-EU", "NCT06123457")
            )
        );

        saved.NextPageId.ShouldBe(RecruitingPage);
        RecordClinicalTrial[] trials = await StoredTrials();
        trials
            .Select(static t => (t.StudyName, t.ClinicalTrialsGovNumber))
            .ShouldBe([("ION-UK", "NCT06123456"), ("ION-EU", "NCT06123457")]);
        trials[0]
            .OtherClinicalTrialNumbers.OrderBy(static n => n.DisplayOrder)
            .Select(static n => (n.OtherRegistryNumber, n.DisplayOrder))
            .ShouldBe([("2025-001234-56", 1), ("ISRCTN12345678", 2)]);
        trials[1].OtherClinicalTrialNumbers.ShouldBeEmpty();
    }

    [Fact]
    public async Task SavePage_TrialsWithTheSameTrialNumber_AreBothStored()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("ION-UK", "NCT06123456"), Trial("ION-EU", "NCT06123456"))
        );

        (await StoredTrials())
            .Select(static t => (t.StudyName, t.ClinicalTrialsGovNumber))
            .ShouldBe([("ION-UK", "NCT06123456"), ("ION-EU", "NCT06123456")]);
    }

    [Fact]
    public async Task GetPage_SavedTrials_AreReturnedAsSent()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(DetailsPage, TrialsAnswer(Trial("ION-UK", "NCT06123456", "A1", "B2")));

        RecordPageDto page = await GetPage(DetailsPage);

        page.Answers[Trials]
            .GetRawText()
            .ShouldBe(
                """[{"clinical_trials_gov_number":"NCT06123456","other_registry_numbers":["A1","B2"],"study_name":"ION-UK"}]"""
            );
    }

    [Fact]
    public async Task SavePage_Trials_AuditsTheCollectionAsAnOrderedList()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("ION-UK", "NCT06123456", "2025-001234-56", "ISRCTN12345678"))
        );

        RecordFieldAudit audit = await Context
            .RecordFieldAudits.AsNoTracking()
            .SingleAsync(a => a.FieldPath == Trials, Ct);
        audit.OldValue.ShouldBeNull();
        audit.NewValue.ShouldBe(
            """[{"clinical_trials_gov_number":"NCT06123456","other_registry_numbers":["2025-001234-56","ISRCTN12345678"],"study_name":"ION-UK"}]"""
        );
    }

    [Fact]
    public async Task SavePage_RemovingATrial_KeepsTheOtherTrialAndDeletesItsRegistryNumbers()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(
            DetailsPage,
            TrialsAnswer(
                Trial("First", "NCT00000001", "A1"),
                Trial("Second", "NCT00000002", "B1", "B2")
            )
        );

        await SaveOk(DetailsPage, TrialsAnswer(Trial("Second", "NCT00000002", "B2")));

        RecordClinicalTrial remaining = (await StoredTrials()).Single();
        (remaining.StudyName, remaining.ClinicalTrialsGovNumber).ShouldBe(
            ("Second", "NCT00000002")
        );
        remaining.OtherClinicalTrialNumbers.Single().OtherRegistryNumber.ShouldBe("B2");
        (await Context.OtherClinicalTrialNumbers.CountAsync(Ct)).ShouldBe(1);
    }

    [Fact]
    public async Task SavePage_EditingATrialNumber_UpdatesTheTrialInPlace()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("First", "NCT00000001"), Trial("Second", "NCT00000002"))
        );
        int[] rowIds = [.. (await StoredTrials()).Select(static t => t.Id)];

        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("First", "NCT00000009"), Trial("Second", "NCT00000002"))
        );

        (await StoredTrials())
            .Select(static t => (t.Id, t.StudyName))
            .ShouldBe([(rowIds[0], "First"), (rowIds[1], "Second")]);
    }

    [Fact]
    public async Task SavePage_RemovingTheFirstTrialAndAddingOne_KeepsTheOrder()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("First", "NCT00000001"), Trial("Second", "NCT00000002"))
        );

        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("Second", "NCT00000002"), Trial("Third", "NCT00000003"))
        );

        RecordClinicalTrial[] trials = await StoredTrials();
        trials.Select(static t => t.StudyName).ShouldBe(["Second", "Third"]);
        (await GetPage(DetailsPage))
            .Answers[Trials]
            .EnumerateArray()
            .Select(static t => t.GetProperty("study_name").GetString())
            .ShouldBe(["Second", "Third"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SavePage_ReorderingTrials_PreservesSubmittedOrderAndAllowsIdenticalRetry(
        bool swapNumbers
    )
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(
            DetailsPage,
            TrialsAnswer(Trial("First", "NCT00000001", "A1"), Trial("Second", "NCT00000002", "B1"))
        );
        RecordPageDto before = await GetPage(DetailsPage);
        Dictionary<string, JsonElement> answers = swapNumbers
            ? TrialsAnswer(
                Trial("First", "NCT00000002", "A1"),
                Trial("Second", "NCT00000001", "B1")
            )
            : TrialsAnswer(
                Trial("Second", "NCT00000002", "B1"),
                Trial("First", "NCT00000001", "A1")
            );
        SaveRecordPageCommand command = new()
        {
            FormVersion = FormVersions.Medicine,
            Answers = answers,
            PageHash = before.PageHash,
        };

        (
            await _saveHarness.Service.SavePage(_record.Id, DetailsPage, command, Ct)
        ).ShouldBeSuccess();

        RecordPageDto after = await GetPage(DetailsPage);
        JsonElement.DeepEquals(after.Answers[Trials], answers[Trials]).ShouldBeTrue();
        int audits = await Context.RecordFieldAudits.CountAsync(Ct);

        // A retry carries the original hash, not the one returned by a subsequent GET.
        (
            await _saveHarness.Service.SavePage(_record.Id, DetailsPage, command, Ct)
        ).ShouldBeSuccess();
        (await Context.RecordFieldAudits.CountAsync(Ct)).ShouldBe(audits);
    }

    [Fact]
    public async Task SavePage_BlankRegistryNumbers_AreDropped()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        await SaveOk(DetailsPage, TrialsAnswer(Trial("ION-UK", "NCT06123456", "", "  ", "A1")));

        (await StoredTrials())
            .Single()
            .OtherClinicalTrialNumbers.Single()
            .OtherRegistryNumber.ShouldBe("A1");
    }

    [Fact]
    public async Task SavePage_InvalidTrials_ReportsEachProblemAgainstItsItem()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            DetailsPage,
            TrialsAnswer(
                Trial(" ", "NCT", "A1", "a1"),
                Trial("Second", "NCT1234"),
                Trial("Third", "NCT00000003")
            )
        );

        errors.ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [$"{Trials}[0].study_name"] = "Enter the study name",
                [$"{Trials}[0].clinical_trials_gov_number"] =
                    "Enter the trial number from ClinicalTrials.gov",
                [$"{Trials}[0].other_registry_numbers[1]"] =
                    "Trial number from other registry must be different for each entry",
                [$"{Trials}[1].clinical_trials_gov_number"] = "ID must be 8 digits",
            },
            ignoreOrder: true
        );
        (await Context.RecordClinicalTrials.AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task SavePage_NoTrials_AsksForAtLeastOne()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        (await SaveInvalid(DetailsPage, TrialsAnswer()))[Trials]
            .ShouldBe("Add at least one clinical trial");
    }

    [Fact]
    public async Task SavePage_ItemWithAnUnknownKey_IsTheWrongShape()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        Dictionary<string, object?> item = Trial("ION-UK", "NCT06123456");
        item["trial_phase"] = "PhaseIII";

        (await SaveInvalid(DetailsPage, TrialsAnswer(item)))[Trials]
            .ShouldBe(RecordFormMessages.WrongShape);
    }

    [Fact]
    public async Task SavePage_ChangingTheGateToNo_DeletesTheTrialsAndClearsRecruiting()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(DetailsPage, TrialsAnswer(Trial("ION-UK", "NCT06123456", "A1")));
        await SaveOk(RecruitingPage, Answer(Recruiting, "Yes"));
        (await TaskStatus()).ShouldBe(RecordTaskStatus.Completed);

        await SaveOk(GatePage, Answer(HasTrials, "No"));

        (await Context.RecordClinicalTrials.AnyAsync(Ct)).ShouldBeFalse();
        (await Context.OtherClinicalTrialNumbers.AnyAsync(Ct)).ShouldBeFalse();
        (
            await Context.RecordClinicalTrialInformation.AsNoTracking().SingleAsync(Ct)
        ).RecruitingInUk.ShouldBeNull();
        RecordFieldAudit cleared = await Context
            .RecordFieldAudits.AsNoTracking()
            .SingleAsync(a => a.FieldPath == Trials && a.NewValue == null, Ct);
        cleared.OldValue.ShouldBe(
            """[{"clinical_trials_gov_number":"NCT06123456","other_registry_numbers":["A1"],"study_name":"ION-UK"}]"""
        );
        (await TaskStatus()).ShouldBe(RecordTaskStatus.Completed);
    }

    [Fact]
    public async Task TaskList_YesWithoutTrials_IsInProgress()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));

        (await TaskStatus()).ShouldBe(RecordTaskStatus.InProgress);
    }

    [Fact]
    public async Task SavePage_UnchangedTrials_WritesNoAudit()
    {
        await SaveOk(GatePage, Answer(HasTrials, "Yes"));
        await SaveOk(DetailsPage, TrialsAnswer(Trial("ION-UK", "NCT06123456", "A1")));
        int audits = await Context.RecordFieldAudits.CountAsync(Ct);

        await SaveOk(DetailsPage, TrialsAnswer(Trial(" ION-UK ", "NCT06123456", "A1", "")));

        (await Context.RecordFieldAudits.CountAsync(Ct)).ShouldBe(audits);
    }

    private static Dictionary<string, object?> Trial(
        string studyName,
        string trialNumber,
        params string[] otherNumbers
    ) =>
        new(StringComparer.Ordinal)
        {
            ["study_name"] = studyName,
            ["clinical_trials_gov_number"] = trialNumber,
            ["other_registry_numbers"] = otherNumbers,
        };

    private static Dictionary<string, JsonElement> TrialsAnswer(
        params Dictionary<string, object?>[] trials
    ) => Answer(Trials, trials);

    private async Task<RecordClinicalTrial[]> StoredTrials() =>
        await Context
            .RecordClinicalTrials.AsNoTracking()
            .Include(static t => t.OtherClinicalTrialNumbers)
            .OrderBy(static t => t.DisplayOrder ?? int.MaxValue)
            .ThenBy(static t => t.Id)
            .ToArrayAsync(Ct);

    private async Task<RecordTaskStatus> TaskStatus() =>
        (await _taskListHarness.Service.GetTaskList(_record.Id, Ct))
            .ShouldBeSuccess()
            .Sections.SelectMany(static s => s.Tasks)
            .Single(static t => string.Equals(t.Id, "clinical-trials", StringComparison.Ordinal))
            .Status;

    private ServiceTestHarness<TService> CreateHarness<TService>()
        where TService : notnull
    {
        ServiceTestHarness<TService> harness = new(Context);
        harness
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = _organisation.Id,
                    UserRole = UserRole.Standard,
                }
            )
            .UpdateCurrentTime(_now);
        return harness;
    }

    private async Task<RecordPageDto> GetPage(string pageId) =>
        (await _queryHarness.Service.GetPage(_record.Id, pageId, Ct)).ShouldBeSuccess();

    private async Task<SaveRecordPageResult> Save(
        string pageId,
        Dictionary<string, JsonElement> answers
    )
    {
        RecordPageDto page = await GetPage(pageId);
        return await _saveHarness.Service.SavePage(
            _record.Id,
            pageId,
            new SaveRecordPageCommand
            {
                FormVersion = FormVersions.Medicine,
                Answers = answers,
                PageHash = page.PageHash,
            },
            Ct
        );
    }

    private async Task<SaveRecordPageDto> SaveOk(
        string pageId,
        Dictionary<string, JsonElement> answers
    ) => (await Save(pageId, answers)).ShouldBeSuccess();

    private async Task<IReadOnlyDictionary<string, string>> SaveInvalid(
        string pageId,
        Dictionary<string, JsonElement> answers
    ) =>
        (await Save(pageId, answers))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors;

    private static Dictionary<string, JsonElement> Answer<T>(string questionId, T value) =>
        new(StringComparer.Ordinal) { [questionId] = JsonSerializer.SerializeToElement(value) };
}
