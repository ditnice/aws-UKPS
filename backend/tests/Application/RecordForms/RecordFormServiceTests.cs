using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
using UKPS.Api.Application.RecordForms.Rules;
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

    private static readonly DateTime _now = new(2026, 10, 15, 9, 30, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;
    private BnfChapter _bnfChapter = null!;
    private BnfChapter _bnfSection = null!;
    private TherapeuticArea _area = null!;
    private TherapeuticArea _condition = null!;
    private TherapeuticArea _otherCondition = null!;
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

        _queryHarness = CreateHarness<IRecordFormQueryService>(UserRole.Standard, _organisation.Id);
        _saveHarness = CreateHarness<IRecordFormSaveService>(UserRole.Standard, _organisation.Id);
    }

    [Fact]
    public async Task GetPage_FirstPageOfANewRecord_HasNoAnswersAndNoPreviousPage()
    {
        RecordPageDto page = await GetPage("indication");

        page.FormVersion.ShouldBe(FormVersions.Medicine);
        page.RevisionVersion.ShouldBe(await RevisionVersion());
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
        RecordPageDto page = await GetPage("rare-disease");

        page.PreviousPageId.ShouldBe("cancer");
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

        result.NextPageId.ShouldBe("bnf-chapter");
        MedicinesIndicationDetail row = await IndicationRow();
        row.Indication.ShouldBe("Chronic hepatitis C");

        RecordRevision revision = await Context
            .RecordRevisions.AsNoTracking()
            .SingleAsync(r => r.Id == _revision.Id, Ct);
        revision.Version.ShouldNotBe(versionBefore);

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
        ).NextPageId.ShouldBeNull();

        MedicinesIndicationDetail row = await IndicationRow();
        row.BnfChapterId.ShouldBe(_bnfSection.Id);
        row.IndicationIsPaediatric.ShouldBe(IndicationPaediatricStatus.ExclusivelyAdults);
        row.IndicationIsCancer.ShouldBe(YesNoUnknown.No);
        row.IndicationIsRareDisease.ShouldBe(YesNoUnknown.No);
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
    public async Task SavePage_StaleRevisionVersion_IsAConflict()
    {
        uint staleVersion = await RevisionVersion();
        await SaveOk("indication", Answer(Indication, "First"));

        SaveRecordPageError error = (
            await Save("indication", Answer(Indication, "Second"), staleVersion)
        ).ShouldBeError();

        error.ShouldBeOfType<SaveRecordPageError.RevisionChanged>();
        (await IndicationRow()).Indication.ShouldBe("First");
    }

    [Fact]
    public async Task SavePage_OldFormVersion_IsAConflict()
    {
        SaveRecordPageError error = (
            await Save(
                "indication",
                Answer(Indication, "Text"),
                await RevisionVersion(),
                formVersion: "2000.1.1"
            )
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
            .ShouldBeOfType<SaveRecordPageError.NotDraft>();
    }

    [Fact]
    public async Task GetPage_EarlierRevisions_ReadsTheLatestRevision()
    {
        await AddEarlierPublishedRevision();

        (await GetPage("indication")).RevisionVersion.ShouldBe(await RevisionVersion());
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
        string pageId,
        Dictionary<string, JsonElement> answers,
        uint? revisionVersion = null,
        string formVersion = FormVersions.Medicine
    ) =>
        await _saveHarness.Service.SavePage(
            _record.Id,
            pageId,
            new SaveRecordPageCommand
            {
                FormVersion = formVersion,
                RevisionVersion = revisionVersion ?? await RevisionVersion(),
                Answers = answers,
            },
            Ct
        );

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

    private IQueryable<RecordFieldAudit> FieldAudits() =>
        Context.RecordFieldAudits.AsNoTracking().Where(a => a.RevisionId == _revision.Id);

    private static JsonElement Json<T>(T value) => JsonSerializer.SerializeToElement(value);

    private static Dictionary<string, JsonElement> Answer<T>(string questionId, T value) =>
        new(StringComparer.Ordinal) { [questionId] = Json(value) };
}
