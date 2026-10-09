using Shouldly;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Medicines;
using UKPS.Api.Application.Forms.Options;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Application.Forms;

[Collection(DatabaseCollection.Name)]
public class MedicineFormOptionsTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
    private readonly FormDefinition _form = MedicineFormDefinition.Create();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<IReadOnlyList<QuestionOption>> LoadAsync(
        string questionId,
        params string[] savedValues
    )
    {
        var question = _form
            .Pages.SelectMany(page => page.Questions)
            .Single(q => string.Equals(q.Id, questionId, StringComparison.Ordinal));
        await using var context = Fixture.CreateContext();
        return await question.Options!.LoadAsync(context, savedValues, Ct);
    }

    [Fact]
    public async Task BnfChapter_OffersTopLevelChaptersInOrder_LabelledWithTheirCode()
    {
        var chapterTwo = await AddEntity(
            new BnfChapter
            {
                Code = "2",
                Label = "Cardiovascular system",
                DisplayOrder = 2,
            },
            Ct
        );
        var chapterOne = await AddEntity(
            new BnfChapter
            {
                Code = "1",
                Label = "Gastro-intestinal system",
                DisplayOrder = 1,
            },
            Ct
        );
        await AddEntity(
            new BnfChapter
            {
                Code = "1.1",
                Label = "Dyspepsia",
                ParentId = chapterOne.Id,
                DisplayOrder = 1,
            },
            Ct
        );

        var options = await LoadAsync("medicines_product_detail.bnf_chapter_id");

        options.ShouldBe([
            new QuestionOption($"{chapterOne.Id}", "1: Gastro-intestinal system"),
            new QuestionOption($"{chapterTwo.Id}", "2: Cardiovascular system"),
        ]);
    }

    [Fact]
    public async Task BnfChapter_ExcludesArchivedChaptersUnlessAlreadySaved()
    {
        var archived = await AddEntity(
            new BnfChapter
            {
                Code = "3",
                Label = "Respiratory system",
                IsArchived = true,
            },
            Ct
        );

        (await LoadAsync("medicines_product_detail.bnf_chapter_id")).ShouldBeEmpty();
        (await LoadAsync("medicines_product_detail.bnf_chapter_id", $"{archived.Id}"))
            .Select(option => option.Value)
            .ShouldBe([$"{archived.Id}"]);
    }

    [Fact]
    public async Task TherapeuticArea_OffersLeavesLabelledAsStored()
    {
        var addiction = await AddEntity(
            new TherapeuticArea { Label = "1: Addiction", DisplayOrder = 1 },
            Ct
        );
        var allergy = await AddEntity(
            new TherapeuticArea { Label = "2: Allergy", DisplayOrder = 2 },
            Ct
        );

        var options = await LoadAsync("medicines_product_detail_therapeutic_area");

        options.ShouldBe([
            new QuestionOption($"{addiction.Id}", "1: Addiction"),
            new QuestionOption($"{allergy.Id}", "2: Allergy"),
        ]);
    }

    [Fact]
    public async Task TherapeuticArea_WithChildren_OffersTheChildrenLabelledWithTheirParent()
    {
        var parent = await AddEntity(
            new TherapeuticArea { Label = "Oncology", DisplayOrder = 1 },
            Ct
        );
        var child = await AddEntity(
            new TherapeuticArea
            {
                Label = "Lung",
                ParentId = parent.Id,
                DisplayOrder = 1,
            },
            Ct
        );

        var options = await LoadAsync("medicines_product_detail_therapeutic_area");

        options.ShouldBe([new QuestionOption($"{child.Id}", "Oncology › Lung")]);
    }

    [Fact]
    public async Task Paediatric_OffersOptionsInScreenOrder()
    {
        var options = await LoadAsync("medicines_product_detail.indication_is_paediatric");

        options
            .Select(option => option.Value)
            .ShouldBe([
                "ExclusivelyChildren",
                "BothChildrenAndAdults",
                "ExclusivelyAdults",
                "Unknown",
            ]);
    }
}
