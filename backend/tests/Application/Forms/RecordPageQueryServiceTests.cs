using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;
using UKPS.Api.Application.Forms.Rules;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.Forms;

[Collection(DatabaseCollection.Name)]
public class RecordPageQueryServiceTests : DatabaseTestBase
{
    private readonly ServiceTestHarness<IRecordPageQueryService> _harness;
    private MedicineRecord _record = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public RecordPageQueryServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<IRecordPageQueryService>(Context);
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
            }
        );

    private Task<UKPS.Api.Application.Common.Result<RecordPageDto, GetRecordPageError>> GetPage(
        string pageId
    ) => _harness.Service.GetPage(_record.Record.Id, _record.Revision.Id, pageId, Ct);

    private async Task UpdateProductDetail(Action<MedicinesProductDetail> update)
    {
        var context = _harness.GetClearedContext();
        var detail = await context.MedicinesProductDetails.SingleAsync(
            x => x.Id == _record.ProductDetail.Id,
            Ct
        );
        update(detail);
        await context.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task GetPage_ReturnsThePageStructureAndRules()
    {
        var page = (await GetPage("indication")).ShouldBeSuccess();

        page.FormVersion.ShouldBe("2026.10.1");
        page.Section.ShouldBe(
            new FormSectionDto { Id = "indication-details", Title = "Indication details" }
        );
        page.Page.ShouldBe(new FormPageDto { Id = "indication", Title = "Indication" });
        page.PreviousPageId.ShouldBeNull();
        page.OrganisationId.ShouldBe(_record.Organisation.Id);
        page.Context.ShouldBeEmpty();

        var question = page.Questions.ShouldHaveSingleItem();
        question.Id.ShouldBe("medicines_product_detail.indication");
        question.Type.ShouldBe(QuestionType.Textarea);
        question.Label.ShouldBe("What is the indication this product is seeking a licence for?");
        question.Hint.ShouldNotBeNull();
        question.Options.ShouldBeNull();
        question.Rules.ShouldBe([
            new FormRuleDto
            {
                Kind = RuleKind.Required,
                Message = "Enter the indication this product is seeking a licence for",
            },
            new FormRuleDto
            {
                Kind = RuleKind.MaxLength,
                Value = 2000,
                Message = "Indication must be 2000 characters or fewer",
            },
        ]);
    }

    [Fact]
    public async Task GetPage_ReturnsCurrentAnswersForEveryQuestion()
    {
        (await GetPage("indication"))
            .ShouldBeSuccess()
            .Answers["medicines_product_detail.indication"]
            .ShouldBeNull();

        await UpdateProductDetail(x => x.Indication = "Chronic hepatitis C");

        var page = (await GetPage("indication")).ShouldBeSuccess();
        page.Answers.Keys.ShouldBe(["medicines_product_detail.indication"]);
        page.Answers["medicines_product_detail.indication"]!
            .GetValue<string>()
            .ShouldBe("Chronic hepatitis C");
    }

    [Fact]
    public async Task GetPage_EnumAnswer_IsTheEnumName()
    {
        await UpdateProductDetail(x => x.IndicationIsCancer = YesNoUnknown.Unknown);

        var page = (await GetPage("cancer")).ShouldBeSuccess();

        page.Answers["medicines_product_detail.indication_is_cancer"]!
            .GetValue<string>()
            .ShouldBe("Unknown");
        page.PreviousPageId.ShouldBe("paediatric");
        page.Questions.Single().Options!.Select(o => o.Value).ShouldBe(["Yes", "No", "Unknown"]);
    }

    [Fact]
    public async Task GetPage_CheckboxAnswer_IsAnArrayOfIds()
    {
        var area = await AddEntity(new TherapeuticArea { Label = "1: Addiction" }, Ct);
        await AddEntity(
            new MedicinesProductDetailTherapeuticArea
            {
                MedicinesProductDetailId = _record.ProductDetail.Id,
                TherapeuticAreaId = area.Id,
            },
            Ct
        );

        var page = (await GetPage("therapeutic-area")).ShouldBeSuccess();

        var answer = page.Answers["medicines_product_detail_therapeutic_area"]
            .ShouldBeOfType<JsonArray>();
        answer.Select(x => x!.GetValue<string>()).ShouldBe([$"{area.Id}"]);
        page.Questions.Single().Display.ShouldBe("combobox");
    }

    [Fact]
    public async Task GetPage_IncludesAnArchivedOptionOnlyWhenItIsSaved()
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

        (await GetPage("bnf-chapter"))
            .ShouldBeSuccess()
            .Questions.Single()
            .Options!.ShouldBeEmpty();

        await UpdateProductDetail(x => x.BnfChapterId = archived.Id);

        (await GetPage("bnf-chapter"))
            .ShouldBeSuccess()
            .Questions.Single()
            .Options!.ShouldBe([
                new FormOptionDto
                {
                    Value = $"{archived.Id}",
                    Label = "1: Gastro-intestinal system",
                },
            ]);
    }

    [Fact]
    public async Task GetPage_RevisionVersion_IsTheConcurrencyToken()
    {
        var page = (await GetPage("indication")).ShouldBeSuccess();

        var version = await _harness
            .GetClearedContext()
            .RecordRevisions.Where(x => x.Id == _record.Revision.Id)
            .Select(x => x.Version)
            .SingleAsync(Ct);
        version.ShouldNotBe(0u);
        page.RevisionVersion.ShouldBe(version);
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    public async Task GetPage_DraftInOwnOrganisation_IsEditable(UserRole role)
    {
        ActAs(role, _record.Organisation.Id);

        (await GetPage("indication")).ShouldBeSuccess().ReadOnly.ShouldBeFalse();
    }

    [Fact]
    public async Task GetPage_SuperUserInAnotherOrganisation_IsEditable()
    {
        ActAs(UserRole.Super, _record.Organisation.Id + 1);

        (await GetPage("indication")).ShouldBeSuccess().ReadOnly.ShouldBeFalse();
    }

    [Theory]
    [InlineData(WorkflowStatus.InReview)]
    [InlineData(WorkflowStatus.Published)]
    [InlineData(WorkflowStatus.Rejected)]
    public async Task GetPage_RevisionNotDraft_IsReadOnly(WorkflowStatus status)
    {
        var context = _harness.GetClearedContext();
        await context
            .RecordRevisions.Where(x => x.Id == _record.Revision.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(r => r.WorkflowStatus, status), Ct);

        (await GetPage("indication")).ShouldBeSuccess().ReadOnly.ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    public async Task GetPage_RecordInAnotherOrganisation_IsNotAllowed(UserRole role)
    {
        ActAs(role, _record.Organisation.Id + 1);

        (await GetPage("indication"))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.NotAllowed>();
    }

    [Fact]
    public async Task GetPage_UnknownRecord_IsRecordNotFound()
    {
        var result = await _harness.Service.GetPage(999_999, _record.Revision.Id, "indication", Ct);

        result.ShouldBeError().ShouldBeOfType<GetRecordPageError.RecordNotFound>();
    }

    [Fact]
    public async Task GetPage_RevisionOfAnotherRecord_IsRecordNotFound()
    {
        var other = await MedicineRecordTestData.CreateAsync(
            Context,
            Ct,
            _record.Organisation,
            _record.User
        );

        var result = await _harness.Service.GetPage(
            _record.Record.Id,
            other.Revision.Id,
            "indication",
            Ct
        );

        result.ShouldBeError().ShouldBeOfType<GetRecordPageError.RecordNotFound>();
    }

    [Fact]
    public async Task GetPage_UnknownPage_IsPageNotFound()
    {
        (await GetPage("no-such-page"))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.PageNotFound>();
    }

    [Fact]
    public async Task GetPage_RecordTypeWithoutAForm_IsPageNotFound()
    {
        var context = _harness.GetClearedContext();
        await context
            .Records.Where(x => x.Id == _record.Record.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(r => r.RecordType, RecordType.Vaccine), Ct);

        (await GetPage("indication"))
            .ShouldBeError()
            .ShouldBeOfType<GetRecordPageError.PageNotFound>();
    }
}
