using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Dtos;
using UKPS.Api.Application.RecordForms.Errors;
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
/// The names and identifiers page against a real database: answers entered at record creation,
/// edited through the form, with the generic names and other identifiers as two text lists over
/// one collection of rows.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RecordFormNamesAndIdentifiersServiceTests : DatabaseTestBase
{
    private const string PageId = "names-and-identifiers";
    private const string CompanyCode = CreationPageFieldPaths.CompanyCode;
    private const string OtherIdentifiers = CreationPageFieldPaths.OtherIdentifiers;
    private const string BrandedName = CreationPageFieldPaths.BrandedName;
    private const string GenericNames = CreationPageFieldPaths.GenericNames;
    private const string RecordTitle = CreationPageFieldPaths.RecordTitle;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private Record _record = null!;
    private RecordRevision _revision = null!;
    private RecordProductDetail _detail = null!;
    private ServiceTestHarness<IRecordFormQueryService> _queryHarness = null!;
    private ServiceTestHarness<IRecordFormSaveService> _saveHarness = null!;

    public RecordFormNamesAndIdentifiersServiceTests(PostgresFixture fixture)
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
        _revision = await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, _record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, WorkflowStatus.Draft)
                .Generate(),
            Ct
        );
        // As record creation saves it.
        _detail = await AddEntity(
            new RecordProductDetail
            {
                RevisionId = _revision.Id,
                CompanyCode = "GS-123",
                BrandedName = "Epclusa",
                RecordTitle = "Hepatitis C, adults",
                NamesAndIdentifiers =
                [
                    Name("sofosbuvir", NameAndIdentifierType.GenericName, 1),
                    Name("velpatasvir", NameAndIdentifierType.GenericName, 2),
                    Name("GS-7977", NameAndIdentifierType.OtherIdentifier, 1),
                ],
            },
            Ct
        );
        Context.ChangeTracker.Clear();

        _queryHarness = CreateHarness<IRecordFormQueryService>();
        _saveHarness = CreateHarness<IRecordFormSaveService>();
    }

    [Fact]
    public async Task GetPage_ReadsTheAnswersFromRecordCreation()
    {
        RecordPageDto page = await GetPage();

        page.Page.Title.ShouldBe("Provide product record details");
        page.Task.Title.ShouldBe("Names and identifiers");
        page.Section.Title.ShouldBe("Provide product record details");
        page.PreviousPageId.ShouldBeNull();
        page.NextPageId.ShouldBeNull();
        page.Questions.Select(static q => (q.Id, q.Type, q.Multiple))
            .ShouldBe([
                (CompanyCode, FormControl.Text, false),
                (OtherIdentifiers, FormControl.TextList, true),
                (BrandedName, FormControl.Text, false),
                (GenericNames, FormControl.TextList, true),
                (RecordTitle, FormControl.Textarea, false),
            ]);
        page.Questions.Single(static q =>
                string.Equals(q.Id, GenericNames, StringComparison.Ordinal)
            )
            .AddAnotherLabel.ShouldBe("Add another active substance");
        Answer<string>(page, CompanyCode).ShouldBe("GS-123");
        Answer<string[]>(page, OtherIdentifiers).ShouldBe(["GS-7977"]);
        Answer<string>(page, BrandedName).ShouldBe("Epclusa");
        Answer<string[]>(page, GenericNames).ShouldBe(["sofosbuvir", "velpatasvir"]);
        Answer<string>(page, RecordTitle).ShouldBe("Hepatitis C, adults");
    }

    [Fact]
    public async Task SavePage_ChangedGenericNames_RewritesOnlyThoseRowsAndAuditsEachChangedQuestion()
    {
        int otherIdentifierId = (await NameRows())
            .Single(static n => n.NameType == NameAndIdentifierType.OtherIdentifier)
            .Id;

        SaveRecordPageDto result = await SaveOk(
            Answers(
                companyCode: "  GS-124  ",
                genericNames: ["velpatasvir", "sofosbuvir", "", "voxilaprevir"]
            )
        );

        result.NextPageId.ShouldBeNull();
        RecordProductDetail detail = await DetailRow();
        detail.CompanyCode.ShouldBe("GS-124");
        detail.GenericNames.ShouldBe(["velpatasvir", "sofosbuvir", "voxilaprevir"]);
        detail.OtherIdentifiers.ShouldBe(["GS-7977"]);
        (await NameRows())
            .Where(static n => n.NameType == NameAndIdentifierType.GenericName)
            .Select(static n => n.DisplayOrder)
            .ShouldBe([1, 2, 3], ignoreOrder: true);
        (await NameRows())
            .Single(static n => n.NameType == NameAndIdentifierType.OtherIdentifier)
            .Id.ShouldBe(otherIdentifierId);

        (await FieldAudits())
            .Select(static a => (a.FieldPath, a.OldValue, a.NewValue))
            .ShouldBe(
                [
                    (CompanyCode, "GS-123", "GS-124"),
                    (
                        GenericNames,
                        """["sofosbuvir","velpatasvir"]""",
                        """["velpatasvir","sofosbuvir","voxilaprevir"]"""
                    ),
                ],
                ignoreOrder: true
            );
        (await FieldAudits()).ShouldAllBe(a =>
            a.RecordId == _record.Id && a.ChangedBy == _user.Id && a.ChangedAt == _now
        );
    }

    [Fact]
    public async Task SavePage_RemovedOtherIdentifiers_DeletesOnlyThoseRows()
    {
        await SaveOk(Answers(otherIdentifiers: [""], brandedName: null));

        RecordProductDetail detail = await DetailRow();
        detail.OtherIdentifiers.ShouldBeEmpty();
        detail.BrandedName.ShouldBeNull();
        detail.GenericNames.ShouldBe(["sofosbuvir", "velpatasvir"]);
        (await FieldAudits())
            .Select(static a => (a.FieldPath, a.OldValue, a.NewValue))
            .ShouldBe(
                [(OtherIdentifiers, """["GS-7977"]""", null), (BrandedName, "Epclusa", null)],
                ignoreOrder: true
            );
    }

    [Fact]
    public async Task SavePage_NothingChanged_WritesNothing()
    {
        await SaveOk(Answers());

        (await FieldAudits()).ShouldBeEmpty();
    }

    [Fact]
    public async Task SavePage_RepeatedGenericName_IsReportedAgainstTheRepeat()
    {
        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            Answers(genericNames: ["sofosbuvir", "velpatasvir", " SOFOSBUVIR "])
        );

        errors.ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [$"{GenericNames}[2]"] = "Generic names must be distinct",
            }
        );
        (await DetailRow()).GenericNames.ShouldBe(["sofosbuvir", "velpatasvir"]);
    }

    [Fact]
    public async Task SavePage_RequiredAnswersRemoved_AreReported()
    {
        IReadOnlyDictionary<string, string> errors = await SaveInvalid(
            Answers(companyCode: " ", genericNames: ["", "  "], recordTitle: new string('a', 101))
        );

        errors.ShouldBe(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [CompanyCode] = "Enter company code",
                [GenericNames] = "Enter generic name",
                [RecordTitle] = "Record title must be between 1 and 100 characters",
            }
        );
        (await FieldAudits()).ShouldBeEmpty();
    }

    [Fact]
    public async Task SavePage_ListThatIsNotAnArrayOfStrings_IsMalformed()
    {
        Dictionary<string, JsonElement> answers = Answers();
        answers[GenericNames] = Json(new object[] { "sofosbuvir", 3 });

        (await SaveInvalid(answers)).Keys.ShouldBe([GenericNames]);
    }

    private static RecordNameAndIdentifier Name(
        string name,
        NameAndIdentifierType type,
        int displayOrder
    ) =>
        new()
        {
            Name = name,
            NameType = type,
            DisplayOrder = displayOrder,
        };

    private static Dictionary<string, JsonElement> Answers(
        string? companyCode = "GS-123",
        string[]? otherIdentifiers = null,
        string? brandedName = "Epclusa",
        string[]? genericNames = null,
        string? recordTitle = "Hepatitis C, adults"
    ) =>
        new(StringComparer.Ordinal)
        {
            [CompanyCode] = Json(companyCode),
            [OtherIdentifiers] = Json(otherIdentifiers ?? ["GS-7977"]),
            [BrandedName] = Json(brandedName),
            [GenericNames] = Json(genericNames ?? ["sofosbuvir", "velpatasvir"]),
            [RecordTitle] = Json(recordTitle),
        };

    private static T Answer<T>(RecordPageDto page, string questionId) =>
        page.Answers[questionId].Deserialize<T>()!;

    private async Task<RecordProductDetail> DetailRow() =>
        await Context
            .RecordProductDetails.AsNoTracking()
            .Include(static x => x.NamesAndIdentifiers)
            .SingleAsync(x => x.Id == _detail.Id, Ct);

    private async Task<List<RecordNameAndIdentifier>> NameRows() =>
        await Context
            .Set<RecordNameAndIdentifier>()
            .AsNoTracking()
            .Where(x => x.RecordProductDetailId == _detail.Id)
            .ToListAsync(Ct);

    private async Task<List<RecordFieldAudit>> FieldAudits() =>
        await Context
            .RecordFieldAudits.AsNoTracking()
            .Where(a => a.RevisionId == _revision.Id)
            .ToListAsync(Ct);

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

    private async Task<RecordPageDto> GetPage() =>
        (await _queryHarness.Service.GetPage(_record.Id, PageId, Ct)).ShouldBeSuccess();

    private async Task<SaveRecordPageResult> Save(Dictionary<string, JsonElement> answers)
    {
        RecordPageDto page = await GetPage();
        SaveRecordPageResult result = await _saveHarness.Service.SavePage(
            _record.Id,
            PageId,
            new SaveRecordPageCommand
            {
                FormVersion = FormVersions.Medicine,
                Answers = answers,
                PageHash = page.PageHash,
            },
            Ct
        );
        Context.ChangeTracker.Clear();
        return result;
    }

    private async Task<SaveRecordPageDto> SaveOk(Dictionary<string, JsonElement> answers) =>
        (await Save(answers)).ShouldBeSuccess();

    private async Task<IReadOnlyDictionary<string, string>> SaveInvalid(
        Dictionary<string, JsonElement> answers
    ) => (await Save(answers)).ShouldBeError().ShouldBeOfType<SaveRecordPageError.Invalid>().Errors;

    private static JsonElement Json<T>(T value) => JsonSerializer.SerializeToElement(value);
}
