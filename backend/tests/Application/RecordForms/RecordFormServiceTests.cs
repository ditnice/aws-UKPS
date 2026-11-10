using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Rules;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
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
/// The page query and save services against a real database. The happy path follows
/// documentation/form-generation/record-creation-walkthrough.md.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RecordFormServiceTests : DatabaseTestBase
{
    private const string Indication = "medicines_indication_detail.indication";
    private const string BnfChapter = "medicines_indication_detail.bnf_chapter_id";
    private const string TherapeuticAreas = "medicines_indication_detail_therapeutic_area";
    private const string Paediatric = "medicines_indication_detail.indication_is_paediatric";
    private const string IsCoMarketed = "medicines_development_background.is_co_marketed";
    private const string CoMarketingName =
        "medicines_development_background.co_marketing_company_name";

    private const string Formulation = "medicines_indication_detail.formulation_type_id";
    private const string TechnologyStatus =
        "medicines_indication_detail.medicine_technology_status";

    private const string LabPrefix = "medicines_laboratory_testing.";
    private const string HtaPrefix = "record_hta.";
    private const string NiceTaId = "record_hta.nice_ta_development_id";

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

    private static readonly string[] _biosimilarAndNewIndication = ["Biosimilar", "NewIndication"];
    private static readonly string[] _niceAndSmc = ["Nice", "Smc"];
    private static readonly string[] _smcOnly = ["Smc"];

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;
    private BnfChapter _bnfChapter = null!;
    private BnfChapter _bnfSection = null!;
    private TherapeuticArea _area = null!;
    private TherapeuticArea _condition = null!;
    private TherapeuticArea _otherCondition = null!;
    private FormulationType _formulation = null!;
    private PatientPathwayPoint _pathwayPoint = null!;
    private ServiceTestHarness<IRecordFormQueryService> _queryHarness = null!;
    private ServiceTestHarness<IRecordFormSaveService> _saveHarness = null!;

    public RecordFormServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(6);
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
        _revision = await AddRevision(WorkflowStatus.Draft);

        _bnfChapter = await AddEntity(
            new BnfChapter
            {
                Code = "5",
                Label = "Infections",
                DisplayOrder = 1,
            },
            Ct
        );
        _bnfSection = await AddEntity(
            new BnfChapter
            {
                Code = "5.3",
                Label = "Antiviral drugs",
                DisplayOrder = 2,
                ParentId = _bnfChapter.Id,
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
        _condition = await AddEntity(
            new TherapeuticArea
            {
                Code = "10.1",
                Label = "Hepatitis C virus (HCV)",
                DisplayOrder = 2,
                ParentId = _area.Id,
            },
            Ct
        );
        _otherCondition = await AddEntity(
            new TherapeuticArea
            {
                Code = "10.2",
                Label = "HIV",
                DisplayOrder = 3,
                ParentId = _area.Id,
            },
            Ct
        );
        _formulation = await AddEntity(new FormulationType { Label = "Tablet" }, Ct);
        _pathwayPoint = await AddEntity(
            new PatientPathwayPoint { Label = "Initial presentation" },
            Ct
        );

        _queryHarness = CreateHarness<IRecordFormQueryService>(UserRole.Standard, _organisation.Id);
        _saveHarness = CreateHarness<IRecordFormSaveService>(UserRole.Standard, _organisation.Id);
    }

    [Fact]
    public async Task GetPage_FirstPageOfANewRecord_HasNoAnswersAndNoPreviousPage()
    {
        RecordPageDto page = await GetPage("indication");

        page.FormVersion.ShouldBe(FormVersions.Medicine);
        page.Section.Title.ShouldBe("Indication details");
        (page.Task.Id, page.Task.Title).ShouldBe(("indication-details", "Indication details"));
        page.PreviousPageId.ShouldBeNull();
        page.NextPageId.ShouldBe("bnf-chapter");
        page.Questions.Single().Id.ShouldBe(Indication);
        page.Answers[Indication].ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetPage_LastPageOfTheTask_HasNoNextPage()
    {
        RecordPageDto page = await GetPage("technology-status");

        page.PreviousPageId.ShouldBe("personalised-medicine");
        page.NextPageId.ShouldBeNull();
    }

    [Fact]
    public async Task GetPage_HierarchicalQuestion_OffersParentsWithChildren()
    {
        RecordPageDto page = await GetPage("bnf-chapter");

        page.PreviousPageId.ShouldBe("indication");
        RecordPageOptionDto chapter = page.Questions.Single().Options!.Single();
        chapter.Value.ShouldBe($"{_bnfChapter.Id}");
        chapter.Label.ShouldBe("5: Infections");
        chapter.Children!.Single().Label.ShouldBe("5.3: Antiviral drugs");
    }

    [Fact]
    public async Task GetPage_ArchivedItems_AreOnlyOfferedWhenSaved()
    {
        _otherCondition.IsArchived = true;
        await Context.SaveChangesAsync(Ct);

        RecordPageDto unsaved = await GetPage("therapeutic-area");
        unsaved
            .Questions.Single()
            .Options!.Single()
            .Children!.Select(static c => c.Value)
            .ShouldBe([$"{_condition.Id}"]);

        await AddEntity(
            new MedicinesIndicationDetail
            {
                RevisionId = _revision.Id,
                TherapeuticAreas =
                [
                    new MedicinesIndicationDetailTherapeuticArea
                    {
                        TherapeuticAreaId = _otherCondition.Id,
                    },
                ],
            },
            Ct
        );

        RecordPageDto saved = await GetPage("therapeutic-area");
        saved
            .Questions.Single()
            .Options!.Single()
            .Children!.Select(static c => c.Value)
            .ShouldBe([$"{_condition.Id}", $"{_otherCondition.Id}"]);
    }

    [Fact]
    public async Task GetPage_UnknownPageOrRecord_IsNotFound()
    {
        (await _queryHarness.Service.GetPage(_record.Id, "unknown", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();
        (await _queryHarness.Service.GetPage(_record.Id + 1, "indication", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotFound>();
    }

    [Fact]
    public async Task GetPage_AnotherOrganisationsRecord_IsNotAllowed()
    {
        var harness = CreateHarness<IRecordFormQueryService>(
            UserRole.Standard,
            _organisation.Id + 100
        );

        (await harness.Service.GetPage(_record.Id, "indication", Ct))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotAllowed>();
    }

    [Fact]
    public async Task SavePage_FirstAnswer_CreatesTheRowAndAuditsTheChange()
    {
        uint versionBefore = await RevisionVersion();

        SaveRecordPageDto result = await SaveOk(
            "indication",
            Answer(Indication, "  Chronic hepatitis C  ")
        );

        (await RevisionVersion()).ShouldNotBe(versionBefore);

        result.NextPageId.ShouldBe("bnf-chapter");
        MedicinesIndicationDetail row = await IndicationRow();
        row.Indication.ShouldBe("Chronic hepatitis C");

        RecordFieldAudit audit = await FieldAudits().SingleAsync(Ct);
        audit.RecordId.ShouldBe(_record.Id);
        audit.FieldPath.ShouldBe(Indication);
        audit.OldValue.ShouldBeNull();
        audit.NewValue.ShouldBe("Chronic hepatitis C");
        audit.ChangedBy.ShouldBe(_user.Id);
        audit.ChangedAt.ShouldBe(_now);
    }

    [Fact]
    public async Task SavePage_StepsThroughEveryPage_StoringEachAnswerType()
    {
        await SaveOk("indication", Answer(Indication, "Hepatitis C"));
        (await SaveOk("bnf-chapter", Answer(BnfChapter, $"{_bnfSection.Id}"))).NextPageId.ShouldBe(
            "therapeutic-area"
        );
        (
            await SaveOk(
                "therapeutic-area",
                Answer(TherapeuticAreas, new[] { $"{_condition.Id}", $"{_area.Id}" })
            )
        ).NextPageId.ShouldBe("paediatric");
        (await SaveOk("paediatric", Answer(Paediatric, "ExclusivelyAdults"))).NextPageId.ShouldBe(
            "cancer"
        );
        await SaveOk("cancer", Answer("medicines_indication_detail.indication_is_cancer", "No"));
        (
            await SaveOk(
                "rare-disease",
                Answer("medicines_indication_detail.indication_is_rare_disease", "No")
            )
        ).NextPageId.ShouldBe("formulation");
        (
            await SaveOk("formulation", Answer(Formulation, $"{_formulation.Id}"))
        ).NextPageId.ShouldBe("presentation");
        await SaveOk("presentation", Answer("medicines_indication_detail.presentation", "10mg"));
        await SaveOk(
            "mode-of-action",
            Answer("medicines_indication_detail.mode_of_action", "Mode")
        );
        await SaveOk(
            "dose-regimen",
            Answer("medicines_indication_detail.proposed_dose_regimen", "Dose")
        );
        await SaveOk(
            "personalised-medicine",
            Answer("medicines_indication_detail.is_personalised_medicine", "Yes")
        );
        (
            await SaveOk("technology-status", Answer(TechnologyStatus, _biosimilarAndNewIndication))
        ).NextPageId.ShouldBeNull();

        MedicinesIndicationDetail row = await IndicationRow();
        row.BnfChapterId.ShouldBe(_bnfSection.Id);
        row.IndicationIsPaediatric.ShouldBe(IndicationPaediatricStatus.ExclusivelyAdults);
        row.IndicationIsCancer.ShouldBe(YesNoUnknown.No);
        row.IndicationIsRareDisease.ShouldBe(YesNoUnknown.No);
        row.FormulationTypeId.ShouldBe(_formulation.Id);
        row.Presentation.ShouldBe("10mg");
        row.ModeOfAction.ShouldBe("Mode");
        row.ProposedDoseRegimen.ShouldBe("Dose");
        row.IsPersonalisedMedicine.ShouldBe(YesNoUnknown.Yes);
        row.MedicineTechnologyStatus.ShouldBe(
            MedicineTechnologyStatus.Biosimilar | MedicineTechnologyStatus.NewIndication
        );
        row.TherapeuticAreas.Select(static t => t.TherapeuticAreaId)
            .Order()
            .ShouldBe(new[] { _area.Id, _condition.Id }.Order());

        List<RecordFieldAudit> changes = await FieldAudits()
            .OrderBy(static a => a.Id)
            .ToListAsync(Ct);
        changes
            .Select(static c => c.FieldPath)
            .ShouldBe([
                Indication,
                BnfChapter,
                TherapeuticAreas,
                Paediatric,
                "medicines_indication_detail.indication_is_cancer",
                "medicines_indication_detail.indication_is_rare_disease",
                Formulation,
                "medicines_indication_detail.presentation",
                "medicines_indication_detail.mode_of_action",
                "medicines_indication_detail.proposed_dose_regimen",
                "medicines_indication_detail.is_personalised_medicine",
                TechnologyStatus,
            ]);
        changes[1].NewValue.ShouldBe($"{_bnfSection.Id}");
        changes[2]
            .NewValue.ShouldBe(
                JsonSerializer.Serialize(
                    new[] { $"{_area.Id}", $"{_condition.Id}" }.Order(StringComparer.Ordinal)
                )
            );
        changes[3].NewValue.ShouldBe("ExclusivelyAdults");
    }

    [Fact]
    public async Task SavePage_StepsThroughEveryPatientAndClinicalRequirementsPage_StoringEachAnswer()
    {
        UkPatientPopulationRange range = await AddEntity(
            new UkPatientPopulationRange { Label = "Between 25 and 50 per 100,000", SortOrder = 3 },
            Ct
        );

        (
            await SaveOk(
                "screening-programme",
                new(StringComparer.Ordinal)
                {
                    ["medicines_patient_identification.screening_required"] = Json("No"),
                    ["medicines_patient_identification.screening_details"] = Json<string?>(null),
                }
            )
        ).NextPageId.ShouldBe("urgent-identification");
        (
            await SaveOk(
                "urgent-identification",
                new(StringComparer.Ordinal)
                {
                    ["medicines_patient_identification.urgent_identification_required"] = Json(
                        "Yes"
                    ),
                    ["medicines_patient_identification.urgent_identification_details"] = Json(
                        "Time-critical"
                    ),
                }
            )
        ).NextPageId.ShouldBe("place-in-therapy");
        (
            await SaveOk(
                "place-in-therapy",
                Answer("medicines_treatment_detail.proposed_place_in_therapy", "Second line")
            )
        ).NextPageId.ShouldBe("treatment-duration");
        (
            await SaveOk(
                "treatment-duration",
                Answer("medicines_treatment_detail.estimated_duration_of_treatment", "6 months")
            )
        ).NextPageId.ShouldBe("nhs-service-changes");
        (
            await SaveOk(
                "nhs-service-changes",
                new(StringComparer.Ordinal)
                {
                    ["medicines_service_impact.nhs_service_changes_required"] = Json("SomeChange"),
                    ["medicines_service_impact.nhs_service_changes_details"] = Json(
                        "Extra staffing"
                    ),
                }
            )
        ).NextPageId.ShouldBe("handling-storage");
        (
            await SaveOk(
                "handling-storage",
                new(StringComparer.Ordinal)
                {
                    ["medicines_service_impact.handling_storage_requirements"] = Json("Unknown"),
                    ["medicines_service_impact.handling_storage_details"] = Json<string?>(null),
                }
            )
        ).NextPageId.ShouldBe("population-range");
        (
            await SaveOk(
                "population-range",
                new(StringComparer.Ordinal)
                {
                    ["medicines_service_impact.uk_patient_population_range_id"] = Json(
                        $"{range.Id}"
                    ),
                    ["medicines_service_impact.uk_patient_population_notes"] = Json("Notes"),
                }
            )
        ).NextPageId.ShouldBe("eligible-population");
        (
            await SaveOk(
                "eligible-population",
                new(StringComparer.Ordinal)
                {
                    ["medicines_service_impact.estimated_eligible_patient_population"] = Json(
                        "1,000 to 2,000"
                    ),
                }
            )
        ).NextPageId.ShouldBeNull();

        MedicinesPatientIdentification identification = await Context
            .MedicinesPatientIdentifications.AsNoTracking()
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);
        identification.UrgentIdentificationRequired.ShouldBe(YesNoUnknown.Yes);
        identification.ScreeningRequired.ShouldBe(YesNoUnknown.No);
        identification.UrgentIdentificationDetails.ShouldBe("Time-critical");

        MedicinesTreatmentDetail treatmentDetail = await Context
            .MedicinesTreatmentDetails.AsNoTracking()
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);
        treatmentDetail.ProposedPlaceInTherapy.ShouldBe("Second line");
        treatmentDetail.EstimatedDurationOfTreatment.ShouldBe("6 months");

        MedicinesServiceImpact serviceImpact = await Context
            .MedicinesServiceImpacts.AsNoTracking()
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);
        serviceImpact.NhsServiceChangesRequired.ShouldBe(NhsServiceChangesRequired.SomeChange);
        serviceImpact.NhsServiceChangesDetails.ShouldBe("Extra staffing");
        serviceImpact.HandlingStorageRequirements.ShouldBe(YesNoUnknown.Unknown);
        serviceImpact.UkPatientPopulationRangeId.ShouldBe(range.Id);
        serviceImpact.UkPatientPopulationNotes.ShouldBe("Notes");
        serviceImpact.EstimatedEligiblePatientPopulation.ShouldBe("1,000 to 2,000");

        (await FieldAudits().CountAsync(Ct)).ShouldBe(11);
    }

    [Fact]
    public async Task SavePage_NothingChanged_WritesNothing()
    {
        await SaveOk("indication", Answer(Indication, "Hepatitis C"));
        uint version = await RevisionVersion();

        await SaveOk("indication", Answer(Indication, " Hepatitis C "));

        (await RevisionVersion()).ShouldBe(version);
        (await FieldAudits().CountAsync(Ct)).ShouldBe(1);
    }

    [Fact]
    public async Task SavePage_FirstSaveWithNothingTicked_IsNotAChange()
    {
        await SaveOk("therapeutic-area", Answer(TherapeuticAreas, Array.Empty<string>()));

        (await Context.MedicinesIndicationDetails.AnyAsync(Ct)).ShouldBeFalse();
        (await FieldAudits().AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task SavePage_ChangedSelection_WritesOnlyTheDifference()
    {
        await SaveOk(
            "therapeutic-area",
            Answer(TherapeuticAreas, new[] { $"{_area.Id}", $"{_condition.Id}" })
        );

        await SaveOk(
            "therapeutic-area",
            Answer(TherapeuticAreas, new[] { $"{_condition.Id}", $"{_otherCondition.Id}" })
        );

        (await IndicationRow())
            .TherapeuticAreas.Select(static t => t.TherapeuticAreaId)
            .Order()
            .ShouldBe(new[] { _condition.Id, _otherCondition.Id }.Order());
        RecordFieldAudit change = await FieldAudits().OrderBy(static a => a.Id).LastAsync(Ct);
        change.OldValue.ShouldBe(
            JsonSerializer.Serialize(
                new[] { $"{_area.Id}", $"{_condition.Id}" }.Order(StringComparer.Ordinal)
            )
        );
    }

    [Fact]
    public async Task SavePage_InvalidAnswers_ReportsOneErrorPerKey()
    {
        var errors = await SaveInvalid(
            "therapeutic-area",
            new(StringComparer.Ordinal)
            {
                [TherapeuticAreas] = Json(new[] { $"{_area.Id}", $"{_area.Id + 1000}" }),
                [Indication] = Json("Not on this page"),
            }
        );

        errors[TherapeuticAreas].ShouldBe(RecordFormMessages.InvalidOption);
        errors[Indication].ShouldBe(RecordFormMessages.NotOnPage);
    }

    [Theory]
    [MemberData(nameof(InvalidIndicationAnswers))]
    public async Task SavePage_InvalidSingleAnswer_ReportsTheProblem(
        string? json,
        string expectedMessage
    )
    {
        Dictionary<string, JsonElement> answers = new(StringComparer.Ordinal);
        if (json is not null)
        {
            answers[Indication] = JsonDocument.Parse(json).RootElement.Clone();
        }

        var errors = await SaveInvalid("indication", answers);

        errors[Indication].ShouldBe(expectedMessage);
    }

    public static TheoryData<string?, string> InvalidIndicationAnswers() =>
        new()
        {
            { null, RecordFormMessages.Missing },
            { "12", RecordFormMessages.WrongShape },
            { "[\"a\"]", RecordFormMessages.WrongShape },
            { "\"   \"", "Enter the indication this product is seeking a licence for" },
            { $"\"{new string('a', 2001)}\"", "Indication must be 2000 characters or fewer" },
        };

    [Fact]
    public async Task SavePage_RulesOnMultiValueQuestions_AreChecked()
    {
        TherapeuticArea extra = await AddEntity(
            new TherapeuticArea
            {
                Code = "11",
                Label = "Non-viral infectious disease",
                DisplayOrder = 4,
            },
            Ct
        );
        string[] tooMany =
        [
            $"{_area.Id}",
            $"{_condition.Id}",
            $"{_otherCondition.Id}",
            $"{extra.Id}",
        ];
        (await SaveInvalid("therapeutic-area", Answer(TherapeuticAreas, tooMany)))[TherapeuticAreas]
            .ShouldBe("Select up to 3 therapeutic areas");

        (
            await SaveInvalid(
                "therapeutic-area",
                Answer(TherapeuticAreas, new[] { $"{_area.Id}", $"{_area.Id}" })
            )
        )[TherapeuticAreas]
            .ShouldBe(RecordFormMessages.Duplicate);
    }

    [Fact]
    public async Task SavePage_OldFormVersion_IsAConflict()
    {
        SaveRecordPageError error = (
            await Save("indication", Answer(Indication, "Text"), formVersion: "2000.1.1")
        ).ShouldBeError();

        error.ShouldBeOfType<SaveRecordPageError.FormVersionChanged>();
    }

    [Fact]
    public async Task SavePage_SubmittedRevision_IsAConflict()
    {
        _revision.WorkflowStatus = WorkflowStatus.QAReview;
        await Context.SaveChangesAsync(Ct);

        (await Save("indication", Answer(Indication, "Text")))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.NotEditable>();
    }

    [Theory]
    [InlineData(RecordStatus.OnHold)]
    [InlineData(RecordStatus.Archived)]
    public async Task SavePage_DraftOfALockedRecord_IsAConflictAndWritesNothing(
        RecordStatus recordStatus
    )
    {
        await SetRecordStatus(recordStatus);

        (await Save("indication", Answer(Indication, "Text")))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.NotEditable>();
        (await FieldAudits().AnyAsync(Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task SavePage_DraftOfAPublishedRecord_Saves()
    {
        await SetRecordStatus(RecordStatus.Published);

        await SaveOk("indication", Answer(Indication, "Text"));

        (await IndicationRow()).Indication.ShouldBe("Text");
    }

    [Fact]
    public async Task SavePage_EarlierRevisions_WritesTheLatestRevision()
    {
        RecordRevision earlier = await AddEarlierPublishedRevision();

        await SaveOk("indication", Answer(Indication, "Text"));

        (await IndicationRow()).Indication.ShouldBe("Text");
        (
            await Context.RecordFieldAudits.AnyAsync(a => a.RevisionId == earlier.Id, Ct)
        ).ShouldBeFalse();
    }

    [Fact]
    public async Task SavePage_AnotherOrganisationsRecord_IsNotAllowed()
    {
        _saveHarness = CreateHarness<IRecordFormSaveService>(
            UserRole.Champion,
            _organisation.Id + 100
        );

        (await Save("indication", Answer(Indication, "Text")))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.NotAllowed>();
    }

    [Fact]
    public async Task SavePage_ChampionOfTheOrganisation_Saves()
    {
        _saveHarness = CreateHarness<IRecordFormSaveService>(UserRole.Champion, _organisation.Id);

        await SaveOk("indication", Answer(Indication, "Hepatitis C"));

        (await IndicationRow()).Indication.ShouldBe("Hepatitis C");
    }

    [Fact]
    public async Task GetPage_SuperUser_CanReadAnyOrganisationsRecord()
    {
        _queryHarness = CreateHarness<IRecordFormQueryService>(
            UserRole.Super,
            _organisation.Id + 100
        );

        (await GetPage("indication")).Questions.Single().Id.ShouldBe(Indication);
    }

    [Fact]
    public async Task SavePage_SuperUser_CannotEditAndWritesNothing()
    {
        uint versionBefore = await RevisionVersion();
        _saveHarness = CreateHarness<IRecordFormSaveService>(UserRole.Super, _organisation.Id);

        (await Save("indication", Answer(Indication, "Text")))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.CannotEdit>();

        (
            await Context.MedicinesIndicationDetails.AnyAsync(x => x.RevisionId == _revision.Id, Ct)
        ).ShouldBeFalse();
        (await FieldAudits().AnyAsync(Ct)).ShouldBeFalse();
        (await RevisionVersion()).ShouldBe(versionBefore);
    }

    [Fact]
    public async Task GetPage_RevealedQuestion_DescribesItsCondition()
    {
        RecordPageDto page = await GetPage("co-marketing");

        page.PreviousPageId.ShouldBe("originator");
        page.Questions.Select(static q => q.Id).ShouldBe([IsCoMarketed, CoMarketingName]);
        page.Questions[0].ShowWhen.ShouldBeNull();
        RecordPageConditionDto showWhen = page.Questions[1].ShowWhen.ShouldNotBeNull();
        showWhen.Kind.ShouldBe(ConditionKind.AnswerIn);
        showWhen.QuestionId.ShouldBe(IsCoMarketed);
        showWhen.Values.ShouldBe(["Yes"]);
        (await GetPage("repurposed-medicine")).PreviousPageId.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_HiddenRequiredQuestion_IsNotValidated()
    {
        SaveRecordPageDto result = await SaveOk(
            "co-marketing",
            CoMarketing("No", companyName: null)
        );

        result.NextPageId.ShouldBeNull();
        (await DevelopmentBackgroundRow()).IsCoMarketed.ShouldBe(YesNoUnknown.No);
    }

    [Fact]
    public async Task SavePage_RevealedRequiredQuestion_IsValidated()
    {
        (await SaveInvalid("co-marketing", CoMarketing("Yes", companyName: "  "))).ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [CoMarketingName] = "Enter the co-marketing company name",
            }
        );
    }

    [Fact]
    public async Task SavePage_AnswerThatNoLongerApplies_IsClearedAndAudited()
    {
        await SaveOk("co-marketing", CoMarketing("Yes", companyName: "Acme Pharma"));

        await SaveOk("co-marketing", CoMarketing("No", companyName: "Acme Pharma"));

        MedicinesDevelopmentBackground row = await DevelopmentBackgroundRow();
        row.IsCoMarketed.ShouldBe(YesNoUnknown.No);
        row.CoMarketingCompanyName.ShouldBeNull();
        RecordFieldAudit cleared = await FieldAudits()
            .Where(static a => a.FieldPath == CoMarketingName)
            .OrderBy(static a => a.Id)
            .LastAsync(Ct);
        cleared.OldValue.ShouldBe("Acme Pharma");
        cleared.NewValue.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_HiddenQuestionSentWithAValue_IsStoredEmpty()
    {
        await SaveOk("co-marketing", CoMarketing("No", companyName: "Acme Pharma"));

        (await DevelopmentBackgroundRow()).CoMarketingCompanyName.ShouldBeNull();
        (await FieldAudits().Select(static a => a.FieldPath).ToListAsync(Ct)).ShouldBe([
            IsCoMarketed,
        ]);
    }

    [Fact]
    public async Task SavePage_RevealedOptionalQuestion_MayBeLeftBlank()
    {
        await SaveOk(
            "repurposed-medicine",
            new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                ["medicines_development_background.is_repurposed_medicine"] = Json("Yes"),
                ["medicines_development_background.repurposed_medicine_details"] = Json<string?>(
                    null
                ),
            }
        );

        (await DevelopmentBackgroundRow()).IsRepurposedMedicine.ShouldBe(YesNoUnknown.Yes);
    }

    [Fact]
    public async Task SavePage_AnotherSection_LeavesItsAnswersAlone()
    {
        // A follow-up stored although its trigger hides it, as seeded or older data may be.
        await AddEntity(
            new MedicinesDevelopmentBackground
            {
                RevisionId = _revision.Id,
                IsCoMarketed = YesNoUnknown.No,
                CoMarketingCompanyName = "Acme Pharma",
            },
            Ct
        );

        await SaveOk("indication", Answer(Indication, "Hepatitis C"));

        (await DevelopmentBackgroundRow()).CoMarketingCompanyName.ShouldBe("Acme Pharma");
        (await FieldAudits().Select(static a => a.FieldPath).ToListAsync(Ct)).ShouldBe([
            Indication,
        ]);
    }

    private static Dictionary<string, JsonElement> CoMarketing(
        string isCoMarketed,
        string? companyName
    ) =>
        new(StringComparer.Ordinal)
        {
            [IsCoMarketed] = Json(isCoMarketed),
            [CoMarketingName] = Json(companyName),
        };

    private async Task<MedicinesDevelopmentBackground> DevelopmentBackgroundRow() =>
        await Context
            .MedicinesDevelopmentBackgrounds.AsNoTracking()
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);

    [Fact]
    public async Task GetPage_FlaggedPage_ShowsTheCommercialInConfidenceBadge()
    {
        RecordPageDto page = await GetPage("diagnostic-test-required");

        page.Page.CommercialInConfidence.ShouldBeTrue();
    }

    [Fact]
    public async Task GetPage_OptionWithAHint_ReturnsTheHint()
    {
        await AddGenomicRoute();

        RecordPageDto page = await GetPage("biomarker-type");

        RecordPageOptionDto option = page
            .Questions.Single()
            .Options!.Single(static o =>
                string.Equals(o.Value, "GenomicBiomarker", StringComparison.Ordinal)
            );
        option.Hint.ShouldBe("For example, a gene mutation, chromosomal alteration");
    }

    [Fact]
    public async Task GetPage_PathwayPointPage_OffersTheReferenceData()
    {
        await AddGenomicRoute();

        RecordPageDto page = await GetPage("pathway-point");

        RecordPageOptionDto option = page.Questions.Single().Options!.ShouldHaveSingleItem();
        (option.Value, option.Label).ShouldBe(($"{_pathwayPoint.Id}", "Initial presentation"));
    }

    [Fact]
    public async Task SavePage_HtaSubmissionIncludingNice_GoesToTheAlignedPathwayPage()
    {
        Dictionary<string, JsonElement> answers = HtaSubmissionAnswers(_niceAndSmc);

        SaveRecordPageDto saved = await SaveOk("hta-submission", answers);

        saved.NextPageId.ShouldBe("nice-aligned-pathway");
    }

    [Fact]
    public async Task SavePage_HtaSubmissionWithoutNice_EndsTheTask()
    {
        Dictionary<string, JsonElement> answers = HtaSubmissionAnswers(_smcOnly);

        SaveRecordPageDto saved = await SaveOk("hta-submission", answers);

        saved.NextPageId.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_HtaSubmissionYesWithNoBodiesOrDetails_ReportsBothErrors()
    {
        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            "hta-submission",
            Answer($"{HtaPrefix}medicine_hta_submission_intended", "Yes")
        );

        errors.Keys.ShouldBe(
            [$"{HtaPrefix}medicine_hta_bodies", $"{HtaPrefix}hta_additional_details"],
            ignoreOrder: true
        );
    }

    [Fact]
    public async Task SavePage_NiceTaIdWithTypeAndNumber_StoresTheJoinedValue()
    {
        await SaveOk("hta-submission", HtaSubmissionAnswers(_niceAndSmc));

        await SaveOk("nice-ta-id", Answer(NiceTaId, "GID-TA10234"));

        (await HtaRow()).NiceTaDevelopmentId.ShouldBe("GID-TA10234");
    }

    [Fact]
    public async Task SavePage_NiceTaIdWithTypeOnly_ClearsAnEarlierValue()
    {
        await SaveOk("hta-submission", HtaSubmissionAnswers(_niceAndSmc));
        await SaveOk("nice-ta-id", Answer(NiceTaId, "GID-TA10234"));

        await SaveOk("nice-ta-id", Answer(NiceTaId, "GID-TA"));

        (await HtaRow()).NiceTaDevelopmentId.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_NiceTaIdWithNumberOnly_ReportsThatATypeIsNeeded()
    {
        await SaveOk("hta-submission", HtaSubmissionAnswers(_niceAndSmc));

        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            "nice-ta-id",
            Answer(NiceTaId, "10234")
        );

        errors[NiceTaId].ShouldBe(RecordFormMessages.SelectIdType);
    }

    [Fact]
    public async Task SavePage_NiceTaIdWithLettersAfterTheType_ReportsThatOnlyNumbersAreAllowed()
    {
        await SaveOk("hta-submission", HtaSubmissionAnswers(_niceAndSmc));

        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            "nice-ta-id",
            Answer(NiceTaId, "GID-TA12a")
        );

        errors[NiceTaId].ShouldBe(RecordFormMessages.IdDigitsOnly);
    }

    [Fact]
    public async Task SavePage_FirstLaboratoryTestingSave_CreatesTheRow()
    {
        (
            await Context.MedicinesLaboratoryTestings.AnyAsync(
                x => x.RevisionId == _revision.Id,
                Ct
            )
        ).ShouldBeFalse();

        await SaveOk(
            "diagnostic-test-required",
            Answer($"{LabPrefix}diagnostic_test_required", "Yes")
        );

        MedicinesLaboratoryTesting row = await LaboratoryTestingRow();
        row.DiagnosticTestRequired.ShouldBe(YesNoUnknown.Yes);
        row.BiomarkerType.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_StepsThroughEveryLaboratoryTestingPage_StoringAndAuditingEachAnswer()
    {
        (string PageId, string QuestionId, string Value)[] answers =
        [
            ("diagnostic-test-required", $"{LabPrefix}diagnostic_test_required", "Yes"),
            ("biomarker-type", $"{LabPrefix}biomarker_type", "GenomicBiomarker"),
            // Genomic route: the non-genomic-biomarker page does not apply and is skipped.
            ("genomic-target", $"{LabPrefix}genomic_target", "EGFR"),
            ("ngtd-relationship", $"{LabPrefix}genomic_test_ngtd_relationship", "NewTest"),
            ("sample-type", $"{LabPrefix}genomic_sample_type", "Tissue"),
            ("turnaround-time", $"{LabPrefix}genomic_turnaround_time_details", "14 days"),
            ("pathway-point", $"{LabPrefix}patient_pathway_point_id", $"{_pathwayPoint.Id}"),
            ("genomic-alterations", $"{LabPrefix}genomic_alterations", "Exon 19 deletion"),
            ("additional-genomic-factors", $"{LabPrefix}additional_genomic_factors", "T790M"),
            ("test-used-in-trials", $"{LabPrefix}genomic_test_used_in_trials", "PCR assay"),
            (
                "specificity-sensitivity",
                $"{LabPrefix}genomic_test_specificity_sensitivity",
                "95% and 90%"
            ),
            (
                "test-mandatory",
                $"{LabPrefix}genomic_test_mandatory_status",
                "MandatoryNoAlternatives"
            ),
            ("monitoring-tests", $"{LabPrefix}monitoring_tests_details", "Quarterly ctDNA"),
            ("safety-tests", $"{LabPrefix}safety_tests_details", "Monthly liver function"),
        ];

        string notesQuestion = $"{LabPrefix}genomic_test_notes";

        for (int i = 0; i < answers.Length; i++)
        {
            Dictionary<string, JsonElement> page = Answer(answers[i].QuestionId, answers[i].Value);
            if (string.Equals(answers[i].PageId, "test-mandatory", StringComparison.Ordinal))
            {
                // Mandatory testing shows the notes box, which is then required.
                page[notesQuestion] = Json("Repeat testing at relapse");
            }

            SaveRecordPageDto saved = await SaveOk(answers[i].PageId, page);
            saved.NextPageId.ShouldBe(i + 1 < answers.Length ? answers[i + 1].PageId : null);
        }

        MedicinesLaboratoryTesting row = await LaboratoryTestingRow();
        row.DiagnosticTestRequired.ShouldBe(YesNoUnknown.Yes);
        row.BiomarkerType.ShouldBe(BiomarkerType.GenomicBiomarker);
        row.NonGenomicBiomarkerDescription.ShouldBeNull();
        row.GenomicTarget.ShouldBe("EGFR");
        row.GenomicTestNgtdRelationship.ShouldBe(GenomicTestNgtdRelationship.NewTest);
        row.GenomicSampleType.ShouldBe("Tissue");
        row.GenomicTurnaroundTimeDetails.ShouldBe("14 days");
        row.PatientPathwayPointId.ShouldBe(_pathwayPoint.Id);
        row.GenomicAlterations.ShouldBe("Exon 19 deletion");
        row.AdditionalGenomicFactors.ShouldBe("T790M");
        row.GenomicTestUsedInTrials.ShouldBe("PCR assay");
        row.GenomicTestSpecificitySensitivity.ShouldBe("95% and 90%");
        row.GenomicTestMandatoryStatus.ShouldBe(GenomicTestMandatoryStatus.MandatoryNoAlternatives);
        row.GenomicTestNotes.ShouldBe("Repeat testing at relapse");
        row.MonitoringTestsDetails.ShouldBe("Quarterly ctDNA");
        row.SafetyTestsDetails.ShouldBe("Monthly liver function");

        List<RecordFieldAudit> changes = await FieldAudits()
            .OrderBy(static a => a.Id)
            .ToListAsync(Ct);
        changes
            .Select(static c => (c.FieldPath, c.NewValue))
            .ShouldBe(
                answers
                    .Select(static a => (a.QuestionId, (string?)a.Value))
                    .SelectMany(a =>
                        string.Equals(
                            a.QuestionId,
                            $"{LabPrefix}genomic_test_mandatory_status",
                            StringComparison.Ordinal
                        )
                            ? new[] { a, (notesQuestion, (string?)"Repeat testing at relapse") }
                            : new[] { a }
                    )
            );
    }

    [Fact]
    public async Task SavePage_PathwayPointThatIsNotOffered_IsRejected()
    {
        await AddGenomicRoute();
        string question = $"{LabPrefix}patient_pathway_point_id";

        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            "pathway-point",
            Answer(question, $"{_pathwayPoint.Id + 100}")
        );

        errors.ShouldContainKey(question);
        (await LaboratoryTestingRow()).PatientPathwayPointId.ShouldBeNull();
    }

    // The route to the genomic pages: they apply only once these two answers are saved.
    private async Task AddGenomicRoute() =>
        await AddEntity(
            new MedicinesLaboratoryTesting
            {
                RevisionId = _revision.Id,
                DiagnosticTestRequired = YesNoUnknown.Yes,
                BiomarkerType = BiomarkerType.GenomicBiomarker,
            },
            Ct
        );

    [Fact]
    public async Task SavePage_WhenThePageHasBeenChangedAfterTheRead_ShouldReturnConcurrencyError()
    {
        RecordPageDto page = await GetPage("indication");

        SaveRecordPageResult firstSaveResult = await Save(page, Answer(Indication, "AnswerA"));
        firstSaveResult.ShouldBeSuccess();

        SaveRecordPageResult secondSaveResult = await Save(page, Answer(Indication, "AnswerB"));
        secondSaveResult.ShouldBeError().ShouldBeOfType<SaveRecordPageError.PageAnswersChanged>();
    }

    [Fact]
    public async Task SavePage_WhenAnswerIsUnchangedExceptForWhitespace_ShouldBeSuccess()
    {
        RecordPageDto page = await GetPage("indication");

        SaveRecordPageResult firstSaveResult = await Save(page, Answer(Indication, "Answer"));
        firstSaveResult.ShouldBeSuccess();

        SaveRecordPageResult secondSaveResult = await Save(
            page,
            Answer(Indication, "   Answer   ")
        );
        secondSaveResult.ShouldBeSuccess();
    }

    [Fact]
    public async Task SavePage_WhenTheAnswerIsEmptyAndUnchanged_ShouldBeSuccess()
    {
        var answer = "Test";
        RecordPageDto page = await GetPage("indication");

        SaveRecordPageResult firstSaveResult = await Save(page, Answer(Indication, answer));
        firstSaveResult.ShouldBeSuccess();

        SaveRecordPageResult secondSaveResult = await Save(page, Answer(Indication, answer));
        secondSaveResult.ShouldBeSuccess();
    }

    [Fact]
    public async Task SavePage_ShouldBeAbleToEditTwoDifferentPagesInParallel()
    {
        RecordPageDto indicationPage = await GetPage("indication");

        RecordPageDto bnfChapterPage = await GetPage("bnf-chapter");
        SaveRecordPageResult bnfChapterUpdate = await Save(
            bnfChapterPage,
            Answer(BnfChapter, $"{_bnfSection.Id}")
        );
        bnfChapterUpdate.ShouldBeSuccess();

        SaveRecordPageResult indicationSaveResult = await Save(
            indicationPage,
            Answer(Indication, "Update")
        );
        indicationSaveResult.ShouldBeSuccess();
    }

    private ServiceTestHarness<TService> CreateHarness<TService>(UserRole role, int organisationId)
        where TService : notnull
    {
        ServiceTestHarness<TService> harness = new(Context);
        harness
            .UpdateCurrentUser(x =>
                x with
                {
                    CognitoUsername = _user.CognitoUsername,
                    OrganisationId = organisationId,
                    UserRole = role,
                }
            )
            .UpdateCurrentTime(_now);
        return harness;
    }

    private async Task<RecordRevision> AddRevision(WorkflowStatus status) =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, status)
                .Generate(),
            Ct
        );

    private async Task SetRecordStatus(RecordStatus status)
    {
        Context.Entry(_record).Property(static x => x.RecordStatus).CurrentValue = status;
        await Context.SaveChangesAsync(Ct);
    }

    private async Task<RecordRevision> AddEarlierPublishedRevision() =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Published)
                .RuleFor(x => x.CreatedAt, _revision.CreatedAt.AddDays(-1))
                .Generate(),
            Ct
        );

    private async Task<RecordPageDto> GetPage(string pageId) =>
        (await _queryHarness.Service.GetPage(_record.Id, pageId, Ct)).ShouldBeSuccess();

    private async Task<SaveRecordPageResult> Save(
        RecordPageDto page,
        Dictionary<string, JsonElement> answers
    )
    {
        return await _saveHarness.Service.SavePage(
            _record.Id,
            page.Page.Id,
            new SaveRecordPageCommand
            {
                FormVersion = page.FormVersion,
                Answers = answers,
                PageHash = page.PageHash,
            },
            Ct
        );
    }

    private async Task<SaveRecordPageResult> Save(
        string pageId,
        Dictionary<string, JsonElement> answers,
        string formVersion = FormVersions.Medicine
    )
    {
        var page = await GetPage(pageId);
        return await _saveHarness.Service.SavePage(
            _record.Id,
            pageId,
            new SaveRecordPageCommand
            {
                FormVersion = formVersion,
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

    private async Task<uint> RevisionVersion() =>
        await Context
            .RecordRevisions.AsNoTracking()
            .Where(r => r.Id == _revision.Id)
            .Select(static r => r.Version)
            .SingleAsync(Ct);

    private async Task<MedicinesIndicationDetail> IndicationRow() =>
        await Context
            .MedicinesIndicationDetails.AsNoTracking()
            .Include(static x => x.TherapeuticAreas)
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);

    private static Dictionary<string, JsonElement> HtaSubmissionAnswers(string[] bodies) =>
        new(StringComparer.Ordinal)
        {
            [$"{HtaPrefix}medicine_hta_submission_intended"] = Json("Yes"),
            [$"{HtaPrefix}medicine_hta_bodies"] = Json(bodies),
            [$"{HtaPrefix}hta_additional_details"] = Json("Planned submissions differ by body"),
        };

    private async Task<RecordHta> HtaRow() =>
        await Context.RecordHtas.AsNoTracking().SingleAsync(x => x.RevisionId == _revision.Id, Ct);

    private async Task<MedicinesLaboratoryTesting> LaboratoryTestingRow() =>
        await Context
            .MedicinesLaboratoryTestings.AsNoTracking()
            .SingleAsync(x => x.RevisionId == _revision.Id, Ct);

    private IQueryable<RecordFieldAudit> FieldAudits() =>
        Context.RecordFieldAudits.AsNoTracking().Where(a => a.RevisionId == _revision.Id);

    private static JsonElement Json<T>(T value) => JsonSerializer.SerializeToElement(value);

    private static Dictionary<string, JsonElement> Answer<T>(string questionId, T value) =>
        new(StringComparer.Ordinal) { [questionId] = Json(value) };
}
