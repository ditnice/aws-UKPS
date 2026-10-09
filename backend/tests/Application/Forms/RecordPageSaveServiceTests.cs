using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.Forms;

[Collection(DatabaseCollection.Name)]
public class RecordPageSaveServiceTests : DatabaseTestBase
{
    private const string IndicationId = "medicines_product_detail.indication";
    private const string CancerId = "medicines_product_detail.indication_is_cancer";
    private const string BnfChapterId = "medicines_product_detail.bnf_chapter_id";
    private const string TherapeuticAreaId = "medicines_product_detail_therapeutic_area";

    private static readonly DateTime _now = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    private readonly ServiceTestHarness<IRecordPageSaveService> _harness;
    private MedicineRecord _record = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public RecordPageSaveServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<IRecordPageSaveService>(Context);
        _harness.UpdateCurrentTime(_now);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _record = await MedicineRecordTestData.CreateAsync(Context, Ct);
        ActAs(UserRole.Standard, _record.Organisation.Id);
    }

    private void ActAs(UserRole role, int organisationId) =>
        _harness.UpdateCurrentUser(x =>
            x with
            {
                UserRole = role,
                OrganisationId = organisationId,
                CognitoUsername = _record.User.CognitoUsername,
            }
        );

    private async Task<uint> GetRevisionVersion() =>
        await _harness
            .GetClearedContext()
            .RecordRevisions.Where(x => x.Id == _record.Revision.Id)
            .Select(x => x.Version)
            .SingleAsync(Ct);

    private async Task<Result<SaveRecordPageDto, SaveRecordPageError>> Save(
        string pageId,
        Dictionary<string, JsonNode?> answers,
        string formVersion = "2026.10.1",
        uint? revisionVersion = null
    ) =>
        await _harness.Service.SavePage(
            _record.Record.Id,
            _record.Revision.Id,
            pageId,
            new SaveRecordPageCommand
            {
                FormVersion = formVersion,
                RevisionVersion = revisionVersion ?? await GetRevisionVersion(),
                Answers = answers,
            },
            Ct
        );

    private static Dictionary<string, JsonNode?> Answer(string questionId, JsonNode? value) =>
        new(StringComparer.Ordinal) { [questionId] = value };

    private async Task<MedicinesProductDetail> GetProductDetail() =>
        await _harness
            .GetClearedContext()
            .MedicinesProductDetails.SingleAsync(x => x.Id == _record.ProductDetail.Id, Ct);

    private async Task<
        List<(string FieldPath, string? OldValue, string? NewValue)>
    > GetFieldChanges() =>
        (
            await _harness
                .GetClearedContext()
                .RecordEvents.Where(x =>
                    x.RevisionId == _record.Revision.Id
                    && x.EventType == RecordEventType.RecordContentUpdated
                )
                .SelectMany(x => x.FieldChanges)
                .OrderBy(x => x.Id)
                .ToListAsync(Ct)
        )
            .Select(x => (x.FieldPath, x.OldValue, x.NewValue))
            .ToList();

    [Fact]
    public async Task SavePage_SavesTheAnswerAndReturnsTheNextPage()
    {
        var result = await Save(
            "indication",
            Answer(IndicationId, JsonValue.Create("  Hepatitis C  "))
        );

        result.ShouldBeSuccess().NextPageId.ShouldBe("bnf-chapter");
        (await GetProductDetail()).Indication.ShouldBe("Hepatitis C");
    }

    [Fact]
    public async Task SavePage_LastPage_HasNoNextPage()
    {
        var result = await Save("cancer", Answer(CancerId, JsonValue.Create("No")));

        result.ShouldBeSuccess().NextPageId.ShouldBeNull();
        (await GetProductDetail()).IndicationIsCancer.ShouldBe(YesNoUnknown.No);
    }

    [Fact]
    public async Task SavePage_AuditsEachChangeWithRawScalarValues()
    {
        await Save("cancer", Answer(CancerId, JsonValue.Create("Unknown")));
        await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        (await GetFieldChanges()).ShouldBe([
            (CancerId, null, "Unknown"),
            (CancerId, "Unknown", "Yes"),
        ]);

        var auditEvent = await _harness
            .GetClearedContext()
            .RecordEvents.Where(x => x.EventType == RecordEventType.RecordContentUpdated)
            .OrderBy(x => x.Id)
            .FirstAsync(Ct);
        auditEvent.RecordId.ShouldBe(_record.Record.Id);
        auditEvent.Note.ShouldBe("cancer");
        auditEvent.PerformedBy.ShouldBe(_record.User.Id);
        auditEvent.PerformedAt.ShouldBe(_now);
    }

    [Fact]
    public async Task SavePage_StampsTheRevisionAndChangesItsVersion()
    {
        var versionBefore = await GetRevisionVersion();

        await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        var revision = await _harness
            .GetClearedContext()
            .RecordRevisions.SingleAsync(x => x.Id == _record.Revision.Id, Ct);
        revision.UpdatedAt.ShouldBe(_now);
        revision.UpdatedBy.ShouldBe(_record.User.Id);
        revision.Version.ShouldNotBe(versionBefore);
    }

    [Fact]
    public async Task SavePage_NothingChanged_WritesNothingAndStillReturnsTheNextPage()
    {
        await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));
        var versionBefore = await GetRevisionVersion();

        var result = await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        result.ShouldBeSuccess().NextPageId.ShouldBeNull();
        (await GetFieldChanges()).Count.ShouldBe(1);
        (await GetRevisionVersion()).ShouldBe(versionBefore);
    }

    [Fact]
    public async Task SavePage_WhitespaceOnlyChange_IsNotAChange()
    {
        await Save("indication", Answer(IndicationId, JsonValue.Create("Hepatitis C")));

        await Save("indication", Answer(IndicationId, JsonValue.Create("Hepatitis C   ")));

        (await GetFieldChanges()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task SavePage_Checkbox_SavesTheSetAndAuditsItAsJson()
    {
        var first = await AddEntity(new TherapeuticArea { Label = "1: Addiction" }, Ct);
        var second = await AddEntity(new TherapeuticArea { Label = "2: Allergy" }, Ct);
        string[] ids = [$"{first.Id}", $"{second.Id}"];
        var expectedJson = new JsonArray([
            .. ids.Order(StringComparer.Ordinal).Select(id => (JsonNode)id),
        ]).ToJsonString();

        await Save("therapeutic-area", Answer(TherapeuticAreaId, new JsonArray(ids[1], ids[0])));
        // The same set in another order is not a change.
        await Save("therapeutic-area", Answer(TherapeuticAreaId, new JsonArray(ids[0], ids[1])));
        await Save("therapeutic-area", Answer(TherapeuticAreaId, new JsonArray()));

        (await GetFieldChanges()).ShouldBe([
            (TherapeuticAreaId, null, expectedJson),
            (TherapeuticAreaId, expectedJson, null),
        ]);
        (
            await _harness
                .GetClearedContext()
                .MedicinesProductDetailTherapeuticAreas.CountAsync(
                    x => x.MedicinesProductDetailId == _record.ProductDetail.Id,
                    Ct
                )
        ).ShouldBe(0);
    }

    [Fact]
    public async Task SavePage_RuleFailures_AreReturnedByQuestionAndNothingIsSaved()
    {
        var result = await Save("indication", Answer(IndicationId, JsonValue.Create("   ")));

        var invalid = result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.Invalid>();
        invalid.Errors.Keys.ShouldBe([IndicationId]);
        invalid
            .Errors[IndicationId]
            .ShouldBe(["Enter the indication this product is seeking a licence for"]);
        (await GetFieldChanges()).ShouldBeEmpty();
    }

    [Fact]
    public async Task SavePage_TooLong_FailsMaxLength()
    {
        var result = await Save(
            "indication",
            Answer(IndicationId, JsonValue.Create(new string('a', 2001)))
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors[IndicationId]
            .ShouldBe(["Indication must be 2000 characters or fewer"]);
    }

    [Fact]
    public async Task SavePage_TooManyItems_FailsMaxItems()
    {
        var areas = Enumerable
            .Range(1, 4)
            .Select(i => new TherapeuticArea { Label = $"{i}" })
            .ToList();
        await AddEntities(areas, Ct);

        var result = await Save(
            "therapeutic-area",
            Answer(TherapeuticAreaId, new JsonArray([.. areas.Select(a => (JsonNode)$"{a.Id}")]))
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors[TherapeuticAreaId]
            .ShouldBe(["Select up to 3 therapeutic areas"]);
    }

    public static TheoryData<string, string> ShapeCases =>
        new()
        {
            { "{}", PageAnswerValidator.MissingAnswer },
            {
                "{\"medicines_product_detail.indication_is_cancer\": 2}",
                PageAnswerValidator.WrongFormat
            },
            {
                "{\"medicines_product_detail.indication_is_cancer\": [\"Yes\"]}",
                PageAnswerValidator.WrongFormat
            },
            {
                "{\"medicines_product_detail.indication_is_cancer\": \"Maybe\"}",
                PageAnswerValidator.InvalidOption
            },
            {
                "{\"medicines_product_detail.indication_is_cancer\": \"yes\"}",
                PageAnswerValidator.InvalidOption
            },
        };

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public async Task SavePage_MalformedAnswer_IsInvalid(string answersJson, string expectedError)
    {
        var answers = JsonNode
            .Parse(answersJson)!
            .AsObject()
            .ToDictionary(x => x.Key, x => x.Value?.DeepClone(), StringComparer.Ordinal);

        var result = await Save("cancer", answers);

        result
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors[CancerId]
            .ShouldBe([expectedError]);
    }

    [Fact]
    public async Task SavePage_UnknownQuestion_IsInvalid()
    {
        var answers = Answer(CancerId, JsonValue.Create("Yes"));
        answers[IndicationId] = JsonValue.Create("Not on this page");

        var result = await Save("cancer", answers);

        result
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors[IndicationId]
            .ShouldBe([PageAnswerValidator.UnknownQuestion]);
        (await GetProductDetail()).IndicationIsCancer.ShouldBeNull();
    }

    [Fact]
    public async Task SavePage_ArchivedOption_IsRejectedUnlessAlreadySaved()
    {
        var archived = await AddEntity(
            new BnfChapter
            {
                Code = "1",
                Label = "Gastro-intestinal system",
                IsArchived = true,
            },
            Ct
        );

        (await Save("bnf-chapter", Answer(BnfChapterId, JsonValue.Create($"{archived.Id}"))))
            .ShouldBeError()
            .ShouldBeOfType<SaveRecordPageError.Invalid>()
            .Errors[BnfChapterId]
            .ShouldBe([PageAnswerValidator.InvalidOption]);

        var context = _harness.GetClearedContext();
        await context
            .MedicinesProductDetails.Where(x => x.Id == _record.ProductDetail.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(d => d.BnfChapterId, archived.Id), Ct);

        (
            await Save("bnf-chapter", Answer(BnfChapterId, JsonValue.Create($"{archived.Id}")))
        ).ShouldBeSuccess();
    }

    [Fact]
    public async Task SavePage_StaleRevisionVersion_IsRevisionChanged()
    {
        var staleVersion = await GetRevisionVersion();
        await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        var result = await Save(
            "cancer",
            Answer(CancerId, JsonValue.Create("No")),
            revisionVersion: staleVersion
        );

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.RevisionChanged>();
        (await GetProductDetail()).IndicationIsCancer.ShouldBe(YesNoUnknown.Yes);
    }

    [Fact]
    public async Task SavePage_ConcurrentChangeDuringSave_IsRevisionChanged()
    {
        // Simulate another save landing between this save's version check and its commit.
        var version = await GetRevisionVersion();
        void ChangeRevisionElsewhere(object? sender, SavingChangesEventArgs e)
        {
            using var otherContext = Fixture.CreateContext();
            otherContext
                .RecordRevisions.Where(x => x.Id == _record.Revision.Id)
                .ExecuteUpdate(x => x.SetProperty(r => r.UpdatedAt, DateTime.UtcNow));
        }

        Context.SavingChanges += ChangeRevisionElsewhere;
        try
        {
            var result = await Save(
                "cancer",
                Answer(CancerId, JsonValue.Create("Yes")),
                revisionVersion: version
            );

            result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.RevisionChanged>();
        }
        finally
        {
            Context.SavingChanges -= ChangeRevisionElsewhere;
        }
    }

    [Fact]
    public async Task SavePage_CanReadButNotEdit_IsEditNotAllowed()
    {
        // No role can read records without also being able to edit them today, so substitute
        // the authoriser to cover the distinction.
        var authoriser = new ReadOnlyAuthoriser(_record.Organisation.Id);
        _harness.ConfigureServices(services =>
            services
                .RemoveAll<IOrganisationAuthoriser>()
                .AddScoped<IOrganisationAuthoriser>(_ => authoriser)
        );

        var result = await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.EditNotAllowed>();
    }

    [Fact]
    public async Task SavePage_FormVersionChanged_IsRejected()
    {
        var result = await Save(
            "cancer",
            Answer(CancerId, JsonValue.Create("Yes")),
            formVersion: "2020.1.1"
        );

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.FormVersionChanged>();
    }

    [Theory]
    [InlineData(WorkflowStatus.InReview)]
    [InlineData(WorkflowStatus.Published)]
    [InlineData(WorkflowStatus.Rejected)]
    public async Task SavePage_RevisionNotDraft_IsRejected(WorkflowStatus status)
    {
        await _harness
            .GetClearedContext()
            .RecordRevisions.Where(x => x.Id == _record.Revision.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(r => r.WorkflowStatus, status), Ct);

        var result = await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.RevisionNotDraft>();
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    public async Task SavePage_RecordInAnotherOrganisation_IsNotAllowed(UserRole role)
    {
        ActAs(role, _record.Organisation.Id + 1);

        var result = await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")));

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.NotAllowed>();
    }

    [Fact]
    public async Task SavePage_SuperUserInAnotherOrganisation_CanEdit()
    {
        ActAs(UserRole.Super, _record.Organisation.Id + 1);

        (await Save("cancer", Answer(CancerId, JsonValue.Create("Yes")))).ShouldBeSuccess();
    }

    [Fact]
    public async Task SavePage_UnknownPage_IsPageNotFound()
    {
        var result = await Save("no-such-page", Answer(CancerId, JsonValue.Create("Yes")));

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.PageNotFound>();
    }

    [Fact]
    public async Task SavePage_RevisionOfAnotherRecord_IsRecordNotFound()
    {
        var other = await MedicineRecordTestData.CreateAsync(
            Context,
            Ct,
            _record.Organisation,
            _record.User
        );

        var result = await _harness.Service.SavePage(
            _record.Record.Id,
            other.Revision.Id,
            "cancer",
            new SaveRecordPageCommand
            {
                FormVersion = "2026.10.1",
                RevisionVersion = 0,
                Answers = Answer(CancerId, JsonValue.Create("Yes")),
            },
            Ct
        );

        result.ShouldBeError().ShouldBeOfType<SaveRecordPageError.RecordNotFound>();
    }

    private sealed class ReadOnlyAuthoriser(int organisationId) : IOrganisationAuthoriser
    {
        public ValueOrAll<int> GetAuthorisedOrganisations(Operation operation) =>
            operation == Operation.Read ? organisationId : ValueOrAll<int>.None();
    }
}
