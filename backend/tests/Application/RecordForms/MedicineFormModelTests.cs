using Shouldly;
using UKPS.Api.Application.RecordForms;
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
        "186abcd21d235e1e3b5b66e898b22e3f95f832064ab177c1afc324cc5f995950";

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
    public void Model_HasTheIndicationDetailsPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.SectionId, "indication-details", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe([
                "indication",
                "bnf-chapter",
                "therapeutic-area",
                "paediatric",
                "cancer",
                "rare-disease",
            ]);
    }

    [Fact]
    public void Model_HasTheLaboratoryTestingPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.SectionId, "laboratory-testing", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe([
                "diagnostic-test-required",
                "biomarker-type",
                "non-genomic-biomarker",
                "genomic-target",
                "ngtd-relationship",
                "sample-type",
                "turnaround-time",
                "pathway-point",
                "genomic-alterations",
                "additional-genomic-factors",
                "test-used-in-trials",
                "specificity-sensitivity",
                "test-mandatory",
                "monitoring-tests",
                "safety-tests",
            ]);
    }

    [Fact]
    public void Model_HasTheExpectedQuestions()
    {
        var questions = Model
            .Pages.SelectMany(static p => p.Questions)
            .Select(static q => (q.Id, q.Control, q.Shape))
            .ToArray();

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
                "medicines_laboratory_testing.diagnostic_test_required",
                FormControl.Radio,
                AnswerShape.Single
            ),
            ("medicines_laboratory_testing.biomarker_type", FormControl.Radio, AnswerShape.Single),
            (
                "medicines_laboratory_testing.non_genomic_biomarker_description",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            ("medicines_laboratory_testing.genomic_target", FormControl.Text, AnswerShape.Single),
            (
                "medicines_laboratory_testing.genomic_test_ngtd_relationship",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_sample_type",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_turnaround_time_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.patient_pathway_point_id",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_alterations",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.additional_genomic_factors",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_test_used_in_trials",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_test_specificity_sensitivity",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.genomic_test_mandatory_status",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.monitoring_tests_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_laboratory_testing.safety_tests_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
        ]);
    }

    [Fact]
    public void NextAndPreviousPage_FollowFormOrder()
    {
        FormModel model = Model;

        model.PreviousPage(model.Pages[0]).ShouldBeNull();
        model.NextPage(model.FindPage("indication")!)!.Id.ShouldBe("bnf-chapter");
        model.PreviousPage(model.FindPage("bnf-chapter")!)!.Id.ShouldBe("indication");
        model.NextPage(model.FindPage("rare-disease")!).ShouldBeNull();
        model.PreviousPage(model.FindPage("diagnostic-test-required")!).ShouldBeNull();
        model.NextPage(model.FindPage("diagnostic-test-required")!)!.Id.ShouldBe("biomarker-type");
        model.NextPage(model.FindPage("safety-tests")!).ShouldBeNull();
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
