using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Tests.Application.RecordForms;

public class MedicineFormModelTests
{
    // Bump FormVersions.Medicine whenever this changes, then update both values.
    private const string ExpectedVersion = "2026.11.2";
    private const string ExpectedFingerprint =
        "9526150dc146daf6e97d06e8197db77846012301aebf0e3c042ffe2f82a873a2";

    private static readonly OptionProviderRegistry _options =
        OptionProviderRegistry.CreateDefault();

    private static FormModel Model => FormModelFactory.Create(_options).Find(RecordType.Medicine)!;

    [Fact]
    public void Model_IsValid()
    {
        Should.NotThrow(() => FormModelFactory.Create(_options));
        Model.ShouldNotBeNull();
    }

    [Fact]
    public void Model_HasThePagesInOrder()
    {
        Model
            .Pages.Select(static p => (p.SectionId, p.Id))
            .ShouldBe([
                ("indication-details", "indication"),
                ("indication-details", "bnf-chapter"),
                ("indication-details", "therapeutic-area"),
                ("indication-details", "paediatric"),
                ("indication-details", "cancer"),
                ("indication-details", "rare-disease"),
                ("development-background", "repurposed-medicine"),
                ("development-background", "originator"),
                ("development-background", "co-marketing"),
            ]);
    }

    [Fact]
    public void Model_DevelopmentBackgroundRevealsFollowUpQuestions()
    {
        const string Prefix = "medicines_development_background.";
        Model
            .Questions.Where(static q => q.ShowWhen is not null)
            .Select(static q => (q.Id, q.ShowWhen!.Description))
            .ShouldBe([
                (
                    $"{Prefix}repurposed_medicine_details",
                    $"AnswerIn {Prefix}is_repurposed_medicine [Yes]"
                ),
                (
                    $"{Prefix}originator_company_name",
                    $"AnswerIn {Prefix}is_originator_company [No]"
                ),
                ($"{Prefix}co_marketing_company_name", $"AnswerIn {Prefix}is_co_marketed [Yes]"),
            ]);
        Model.Pages.ShouldAllBe(static p => p.ShowWhen == null);
    }

    [Fact]
    public void EveryPage_ReadsAndSavesOnlyItsOwnRows()
    {
        // The Medicine form has only same-page reveals, so conditions add no rows to any request.
        FormModel model = Model;
        foreach (FormPage page in model.Pages)
        {
            model.EntitiesToRead(page).ShouldBe(page.Entities, page.Id);
            model.EntitiesToSave(page).ShouldBe(page.Entities, page.Id);
            model.AffectedBy(page).ShouldBe(page.Questions, page.Id);
        }
    }

    [Fact]
    public void Model_HasTheExpectedQuestions()
    {
        var questions = Model.Questions.Select(static q => (q.Id, q.Control, q.Shape)).ToArray();

        questions.ShouldBe([
            ("medicines_indication_detail.indication", FormControl.Textarea, AnswerShape.Single),
            (
                "medicines_indication_detail.bnf_chapter_id",
                FormControl.HierarchicalSelect,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail_therapeutic_area",
                FormControl.HierarchicalSelect,
                AnswerShape.Multi
            ),
            (
                "medicines_indication_detail.indication_is_paediatric",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail.indication_is_cancer",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail.indication_is_rare_disease",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.is_repurposed_medicine",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.repurposed_medicine_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.is_originator_company",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.originator_company_name",
                FormControl.Text,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.is_co_marketed",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_development_background.co_marketing_company_name",
                FormControl.Text,
                AnswerShape.Single
            ),
        ]);
    }

    [Fact]
    public void NextAndPreviousPage_FollowFormOrder()
    {
        FormModel model = Model;
        Applicability applicability = FormApplicability.Evaluate(
            model,
            model.Questions.ToDictionary(
                static q => q.Id,
                static q => FormAnswer.Empty(q.Shape),
                StringComparer.Ordinal
            )
        );

        model.PreviousPage(model.Pages[0], applicability).ShouldBeNull();
        model
            .NextPage(model.FindPage("indication")!, applicability)
            .ShouldNotBeNull()
            .Id.ShouldBe("bnf-chapter");
        model
            .PreviousPage(model.FindPage("bnf-chapter")!, applicability)
            .ShouldNotBeNull()
            .Id.ShouldBe("indication");
        model.NextPage(model.FindPage("rare-disease")!, applicability).ShouldBeNull();
        model.PreviousPage(model.FindPage("repurposed-medicine")!, applicability).ShouldBeNull();
        model
            .NextPage(model.FindPage("repurposed-medicine")!, applicability)
            .ShouldNotBeNull()
            .Id.ShouldBe("originator");
        model.NextPage(model.FindPage("co-marketing")!, applicability).ShouldBeNull();
        model.FindPage("unknown").ShouldBeNull();
    }

    [Fact]
    public void Model_ChangesRequireAVersionBump()
    {
        string fingerprint = FormModelFingerprint.Compute(Model, _options);

        FormVersions.Medicine.ShouldBe(ExpectedVersion);
        fingerprint.ShouldBe(
            ExpectedFingerprint,
            $"The Medicine form changed. Bump FormVersions.Medicine, then set ExpectedVersion to it and ExpectedFingerprint to {fingerprint}. The model is now:{Environment.NewLine}{FormModelFingerprint.Describe(Model, _options)}"
        );
    }
}
