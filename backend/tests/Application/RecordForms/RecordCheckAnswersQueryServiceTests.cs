using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>Check your answers against a real database, starting from a complete draft.</summary>
[Collection(DatabaseCollection.Name)]
public class RecordCheckAnswersQueryServiceTests : DatabaseTestBase
{
    private const string IndicationIsCancer = "medicines_indication_detail.indication_is_cancer";
    private const string BnfChapter = "medicines_indication_detail.bnf_chapter_id";
    private const string Presentation = "medicines_indication_detail.presentation";
    private const string ScreeningRequired = "medicines_patient_identification.screening_required";
    private const string ScreeningDetails = "medicines_patient_identification.screening_details";
    private const string TherapeuticAreas = "medicines_indication_detail_therapeutic_area";
    private const string UkLicenceDate = "record_mhra_date.uk_licence_date";
    private const string UkLaunchDate = "record_hta.uk_launch_date";
    private const string BiomarkerTypeQuestion = "medicines_laboratory_testing.biomarker_type";
    private const string MhraProcedureQuestion = "record_mhra_procedure.mhra_procedure_type_id";
    private const string Trials = "record_clinical_trial";

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;

    public RecordCheckAnswersQueryServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(586);
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
        _revision = await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
                .Generate(),
            Ct
        );
        await CompleteMedicineRevision.AddAsync(Context, _revision.Id, new(2027, 4, 1), Ct);
    }

    [Fact]
    public async Task GetCheckAnswers_CompleteDraft_HasNoErrorsAndItsHashSubmits()
    {
        RecordCheckAnswersDto dto = await GetCheckAnswers();

        dto.RecordId.ShouldBe(_record.Id);
        dto.OrganisationId.ShouldBe(_organisation.Id);
        dto.FormVersion.ShouldBe(FormVersions.Medicine);
        dto.Errors.ShouldBeEmpty();
        (
            await Harness<IRecordSubmissionService>()
                .Service.Submit(
                    _record.Id,
                    new SubmitRecordCommand
                    {
                        FormVersion = dto.FormVersion,
                        RevisionHash = dto.RevisionHash,
                    },
                    Ct
                )
        ).ShouldBeSuccess();
    }

    [Fact]
    public async Task GetCheckAnswers_QuestionHiddenByItsTrigger_IsLeftOut()
    {
        // Screening details only applies when screening is required, which it isn't.
        RecordCheckAnswersDto dto = await GetCheckAnswers();

        QuestionIds(dto).ShouldContain(ScreeningRequired);
        QuestionIds(dto).ShouldNotContain(ScreeningDetails);
    }

    [Fact]
    public async Task GetCheckAnswers_QuestionRevealedByItsTrigger_IsIncludedAndReported()
    {
        await UpdateRow<MedicinesPatientIdentification>(x =>
            x.ScreeningRequired = YesNoUnknown.Yes
        );

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        Question(dto, ScreeningDetails).Values.ShouldBeEmpty();
        dto.Errors.ShouldHaveSingleItem().QuestionId.ShouldBe(ScreeningDetails);
    }

    [Fact]
    public async Task GetCheckAnswers_QuestionOnAPageHiddenByItsTrigger_IsLeftOutEvenWhenAnswered()
    {
        // The biomarker type page only applies when a diagnostic test is required, which it
        // isn't; the answer saved on it is ignored rather than shown or checked.
        await UpdateRow<MedicinesLaboratoryTesting>(x =>
            x.BiomarkerType = BiomarkerType.GenomicBiomarker
        );

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        QuestionIds(dto).ShouldNotContain(BiomarkerTypeQuestion);
        dto.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCheckAnswers_ChosenValueNoLongerOffered_IsLeftOutOfTheOptionsAndReported()
    {
        // A vaccine-only procedure isn't offered on a medicine.
        MhraProcedureType vaccineOnly = await AddEntity(
            new MhraProcedureType
            {
                Code = "VACCINE_ONLY",
                Label = "Vaccine only",
                RelevantTo = ReferenceDataType.VaccinesOnly,
            },
            Ct
        );
        await UpdateRow<RecordMhraProcedure>(x => x.MhraProcedureTypeId = vaccineOnly.Id);

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        RecordAnswersQuestionDto question = Question(dto, MhraProcedureQuestion);
        question.Values.ShouldBe([$"{vaccineOnly.Id}"]);
        question.SelectedOptions.ShouldNotBeNull().ShouldBeEmpty();
        dto.Errors.ShouldContain(e =>
            string.Equals(e.QuestionId, MhraProcedureQuestion, StringComparison.Ordinal)
        );
    }

    [Fact]
    public async Task GetCheckAnswers_OptionalQuestionNotAnswered_IsIncluded()
    {
        RecordAnswersQuestionDto question = Question(await GetCheckAnswers(), Presentation);

        question.PageId.ShouldBe("presentation");
        question.Values.ShouldBeEmpty();
        question.SelectedOptions.ShouldBeNull();
    }

    [Fact]
    public async Task GetCheckAnswers_ChoiceQuestion_HasTheChosenOptionsLabel()
    {
        RecordAnswersQuestionDto question = Question(await GetCheckAnswers(), IndicationIsCancer);

        question.Values.ShouldBe([nameof(YesNoUnknown.No)]);
        RecordAnswersOptionDto option = question.SelectedOptions.ShouldHaveSingleItem();
        option.Value.ShouldBe(nameof(YesNoUnknown.No));
        option.Label.ShouldBe("No");
        option.ParentLabel.ShouldBeNull();
    }

    [Fact]
    public async Task GetCheckAnswers_ClinicalTrials_AreReturnedAsOneItemEach()
    {
        await CompleteMedicineRevision.AddClinicalTrialAsync(Context, _revision.Id, Ct);
        // The helper adds one trial; a second with two other numbers shows order is kept.
        await AddEntity(
            new RecordClinicalTrial
            {
                RevisionId = _revision.Id,
                DisplayOrder = 2,
                StudyName = "SURVEYOR-2",
                ClinicalTrialsGovNumber = "NCT07654321",
                OtherClinicalTrialNumbers =
                [
                    new OtherClinicalTrialNumber { OtherRegistryNumber = "A1", DisplayOrder = 1 },
                    new OtherClinicalTrialNumber { OtherRegistryNumber = "B2", DisplayOrder = 2 },
                ],
            },
            Ct
        );

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        RecordAnswersQuestionDto trials = Question(dto, Trials);
        trials.ItemLabel.ShouldBe("Clinical trial");
        trials.AddAnotherLabel.ShouldBe("Add another clinical trial");
        IReadOnlyList<RecordAnswersGroupItemDto> items = trials.Items.ShouldNotBeNull();

        trials.Values.ShouldBeEmpty();
        items.Count.ShouldBe(2);

        // Items come back in display order.
        Child(items[0], "study_name").Values.ShouldBe(["SURVEYOR-1"]);
        Child(items[1], "study_name").Values.ShouldBe(["SURVEYOR-2"]);

        // A child row carries what the frontend needs to format it.
        RecordAnswersQuestionDto studyName = Child(items[0], "study_name");
        studyName.Label.ShouldBe("Study name");
        studyName.Type.ShouldBe(FormControl.Text);

        // A text list child keeps its entries in order.
        Child(items[1], "other_registry_numbers").Values.ShouldBe(["A1", "B2"]);
    }

    [Fact]
    public async Task GetCheckAnswers_ChildOfAHierarchicalOption_HasItsParentsLabel()
    {
        int chapterId = await Context
            .BnfChapters.Where(c => c.Code == "5")
            .Select(static c => c.Id)
            .SingleAsync(Ct);
        BnfChapter section = await AddEntity(
            new BnfChapter
            {
                Code = "5.1",
                Label = "Antibacterial drugs",
                DisplayOrder = 1,
                ParentId = chapterId,
            },
            Ct
        );
        await UpdateRow<MedicinesIndicationDetail>(x => x.BnfChapterId = section.Id);

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        RecordAnswersOptionDto option = Question(dto, BnfChapter)
            .SelectedOptions.ShouldHaveSingleItem();
        option.Value.ShouldBe($"{section.Id}");
        option.Label.ShouldBe("5.1: Antibacterial drugs");
        option.ParentLabel.ShouldBe("5: Infections");
        dto.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCheckAnswers_MultiValueChoice_ListsTheOptionsInTheOrderOffered()
    {
        TherapeuticArea[] areas =
        [
            new() { Code = "A", Label = "Cardiology" },
            new() { Code = "B", Label = "Oncology" },
        ];
        await AddEntities(areas, Ct);
        // Offered in the reverse of the order the answer is stored in, which is by ID.
        TherapeuticArea[] stored = [.. areas.OrderBy(a => $"{a.Id}", StringComparer.Ordinal)];
        stored[0].DisplayOrder = 2;
        stored[1].DisplayOrder = 1;
        int detailId = await Context
            .MedicinesIndicationDetails.Where(x => x.RevisionId == _revision.Id)
            .Select(static x => x.Id)
            .SingleAsync(Ct);
        Context.AddRange(
            areas.Select(a => new MedicinesIndicationDetailTherapeuticArea
            {
                MedicinesIndicationDetailId = detailId,
                TherapeuticAreaId = a.Id,
            })
        );
        await Context.SaveChangesAsync(Ct);

        RecordAnswersQuestionDto question = Question(await GetCheckAnswers(), TherapeuticAreas);

        question.Multiple.ShouldBeTrue();
        question.Values.ShouldBe([$"{stored[0].Id}", $"{stored[1].Id}"]);
        question
            .SelectedOptions.ShouldNotBeNull()
            .Select(static o => o.Value)
            .ShouldBe([$"{stored[1].Id}", $"{stored[0].Id}"]);
    }

    [Fact]
    public async Task GetCheckAnswers_CallerWhoCannotEdit_CannotSubmit()
    {
        (
            await Harness<IRecordCheckAnswersQueryService>(role: UserRole.Super)
                .Service.GetCheckAnswers(_record.Id, Ct)
        )
            .ShouldBeError()
            .ShouldBeOfType<GetCheckAnswersError.CannotSubmit>();
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.QAReview)]
    [InlineData(RecordStatus.OnHold, WorkflowStatus.Draft)]
    public async Task GetCheckAnswers_NotASubmittableDraft_IsNotSubmittable(
        RecordStatus recordStatus,
        WorkflowStatus revisionStatus
    )
    {
        await Context
            .Records.Where(r => r.Id == _record.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.RecordStatus, recordStatus), Ct);
        await Context
            .RecordRevisions.Where(r => r.Id == _revision.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.WorkflowStatus, revisionStatus), Ct);

        (await Harness<IRecordCheckAnswersQueryService>().Service.GetCheckAnswers(_record.Id, Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetCheckAnswersError.NotSubmittable>();
    }

    [Fact]
    public async Task GetCheckAnswers_RequiredAnswerMissing_ReportsTheErrorsSubmitWould()
    {
        await UpdateRow<MedicinesIndicationDetail>(x => x.IndicationIsCancer = null);
        RecordCheckAnswersDto dto = await GetCheckAnswers();

        var submitted = await Harness<IRecordSubmissionService>()
            .Service.Submit(
                _record.Id,
                new SubmitRecordCommand
                {
                    FormVersion = dto.FormVersion,
                    RevisionHash = dto.RevisionHash,
                },
                Ct
            );

        SubmissionErrorDto error = dto.Errors.ShouldHaveSingleItem();
        error.QuestionId.ShouldBe(IndicationIsCancer);
        error.PageId.ShouldBe(Question(dto, IndicationIsCancer).PageId);
        submitted
            .ShouldBeError()
            .ShouldBeOfType<SubmitRecordError.Invalid>()
            .Errors.ShouldBe(dto.Errors);
    }

    [Fact]
    public async Task GetCheckAnswers_CrossPageRuleBroken_ReportsItOnBothDates()
    {
        // The licence is a year after the launch quarter, which breaks CR-01.
        await Context
            .RegulatoryDates.Where(d =>
                d.RevisionId == _revision.Id && d.DateEvent == DateEventType.UkLicence
            )
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.DateValue, new DateOnly(2028, 4, 1)), Ct);

        RecordCheckAnswersDto dto = await GetCheckAnswers();

        dto.Errors.Select(static e => e.QuestionId)
            .ShouldBe([UkLicenceDate, UkLaunchDate], ignoreOrder: true);
        dto.Errors.ShouldAllBe(e => e.PageId == Question(dto, e.QuestionId).PageId);
    }

    [Fact]
    public async Task GetCheckAnswers_SectionsAndTasks_HaveOnlyThoseWithQuestionsThatApply()
    {
        RecordCheckAnswersDto dto = await GetCheckAnswers();

        dto.Sections.ShouldNotBeEmpty();
        dto.Sections.ShouldAllBe(section =>
            section.Tasks.Count > 0 && section.Tasks.All(task => task.Questions.Count > 0)
        );
    }

    [Fact]
    public async Task GetCheckAnswers_UnknownRecord_IsNotFound()
    {
        (
            await Harness<IRecordCheckAnswersQueryService>()
                .Service.GetCheckAnswers(_record.Id + 1000, Ct)
        )
            .ShouldBeError()
            .ShouldBeOfType<GetCheckAnswersError.NotFound>();
    }

    [Fact]
    public async Task GetCheckAnswers_AnotherOrganisationsRecord_IsNotAllowed()
    {
        Organisation other = await AddEntity(new OrganisationFaker().Generate(), Ct);

        (
            await Harness<IRecordCheckAnswersQueryService>(organisationId: other.Id)
                .Service.GetCheckAnswers(_record.Id, Ct)
        )
            .ShouldBeError()
            .ShouldBeOfType<GetCheckAnswersError.NotAllowed>();
    }

    private async Task<RecordCheckAnswersDto> GetCheckAnswers() =>
        (
            await Harness<IRecordCheckAnswersQueryService>().Service.GetCheckAnswers(_record.Id, Ct)
        ).ShouldBeSuccess();

    private static IEnumerable<string> QuestionIds(RecordCheckAnswersDto dto) =>
        dto
            .Sections.SelectMany(static s => s.Tasks)
            .SelectMany(static t => t.Questions)
            .Select(static q => q.Id);

    private static RecordAnswersQuestionDto Question(
        RecordCheckAnswersDto dto,
        string questionId
    ) =>
        dto
            .Sections.SelectMany(static s => s.Tasks)
            .SelectMany(static t => t.Questions)
            .Single(q => string.Equals(q.Id, questionId, StringComparison.Ordinal));

    private static RecordAnswersQuestionDto Child(
        RecordAnswersGroupItemDto item,
        string questionId
    ) => item.Questions.Single(q => string.Equals(q.Id, questionId, StringComparison.Ordinal));

    private async Task UpdateRow<TEntity>(Action<TEntity> update)
        where TEntity : class
    {
        Context.ChangeTracker.Clear();
        TEntity row = await Context
            .Set<TEntity>()
            .SingleAsync(x => EF.Property<int>(x, "RevisionId") == _revision.Id, Ct);
        update(row);
        await Context.SaveChangesAsync(Ct);
        Context.ChangeTracker.Clear();
    }

    private ServiceTestHarness<TService> Harness<TService>(
        int? organisationId = null,
        UserRole role = UserRole.Standard
    )
        where TService : notnull
    {
        ServiceTestHarness<TService> harness = new(Context);
        harness
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = organisationId ?? _organisation.Id,
                    UserRole = role,
                }
            )
            .UpdateCurrentTime(_now);
        return harness;
    }
}
