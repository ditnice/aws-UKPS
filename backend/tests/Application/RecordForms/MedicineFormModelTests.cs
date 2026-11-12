using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Tests.Application.RecordForms;

public class MedicineFormModelTests
{
    // Bump FormVersions.Medicine whenever this changes, then update both values.
    private const string ExpectedVersion = "2026.11.13";
    private const string ExpectedFingerprint =
        "316bc9bee3c98571c7d8578fb48c839d41fa1d5cf0c3b5b9f05f9aaa8c003258";

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
                "formulation",
                "presentation",
                "mode-of-action",
                "dose-regimen",
                "personalised-medicine",
                "technology-status",
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
    public void Model_HasThePimAndEamsPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.SectionId, "pim-and-eams", StringComparison.Ordinal)
            )
            .Select(static p => (p.Id, p.CommercialInConfidence))
            .ShouldBe([
                ("pim-designation", true),
                ("eams-submission-date", true),
                ("eams-opinion-date", true),
                ("eams-opinion-decision", true),
            ]);
    }

    [Fact]
    public void Model_HasTheDevelopmentBackgroundPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.SectionId, "development-background", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe(["repurposed-medicine", "originator", "co-marketing"]);
    }

    [Fact]
    public void Model_HasTheHtaAndLaunchPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.SectionId, "hta-and-launch", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe(["hta-submission", "nice-aligned-pathway", "nice-ta-id", "uk-launch-date"]);
    }

    [Fact]
    public void Model_NiceTaIdOffersTheGidPrefixes()
    {
        FormQuestion question = Model.FindQuestion("record_hta.nice_ta_development_id")!;

        question.Prefixes.ShouldBe(["GID-TA", "GID-HST"]);
        question.Placeholder.ShouldBe("Choose ID type");
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
                (
                    "record_hta.medicine_hta_bodies",
                    "AnswerIn record_hta.medicine_hta_submission_intended [Yes]"
                ),
                (
                    "record_hta.hta_additional_details",
                    "AnswerIn record_hta.medicine_hta_submission_intended [Yes]"
                ),
                (
                    "medicines_eams_pim.will_submit_to_eams",
                    "AnswerIn medicines_eams_pim.pim_designation_status [Granted]"
                ),
                (
                    "medicines_laboratory_testing.genomic_test_notes",
                    "AnswerIn medicines_laboratory_testing.genomic_test_mandatory_status [MandatoryNoAlternatives, MandatoryAlternativesMayExist]"
                ),
                (
                    "medicines_patient_identification.screening_details",
                    "AnswerIn medicines_patient_identification.screening_required [Yes]"
                ),
                (
                    "medicines_patient_identification.urgent_identification_details",
                    "AnswerIn medicines_patient_identification.urgent_identification_required [Yes]"
                ),
                (
                    "medicines_service_impact.nhs_service_changes_details",
                    "AnswerIn medicines_service_impact.nhs_service_changes_required [CompleteTransformation, SomeChange]"
                ),
                (
                    "medicines_service_impact.handling_storage_details",
                    "AnswerIn medicines_service_impact.handling_storage_requirements [Yes]"
                ),
            ]);
        Model
            .Pages.Where(static p => p.ShowWhen is not null)
            .Select(static p => p.Id)
            .ShouldBe([
                "nice-aligned-pathway",
                "nice-ta-id",
                "eams-submission-date",
                "eams-opinion-date",
                "eams-opinion-decision",
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
            ]);
    }

    [Fact]
    public void EveryPage_ReadsAndSavesOnlyItsOwnRows()
    {
        // Reveals are same-page, so they add no rows. Page conditions add one thing to every
        // request: the entities holding the triggers, which navigation needs to know which pages apply.
        FormModel model = Model;
        FormEntity[] triggerEntities = model
            .Pages.Select(static p => p.ShowWhen)
            .OfType<FormCondition>()
            .SelectMany(static c => c.TriggerQuestionIds)
            .Select(id =>
                model
                    .Questions.Single(q => string.Equals(q.Id, id, StringComparison.Ordinal))
                    .Entity
            )
            .Distinct()
            .ToArray();
        foreach (FormPage page in model.Pages)
        {
            FormEntity[] expected = page.Entities.Union(triggerEntities).ToArray();
            model.EntitiesToRead(page).ShouldBe(expected, ignoreOrder: true, page.Id);
            model.EntitiesToSave(page).ShouldBe(expected, ignoreOrder: true, page.Id);

            // Saving a page can only clear its own answers, plus those on pages it makes conditional.
            model
                .AffectedBy(page)
                .ShouldBeSubsetOf(
                    model.Questions.Where(q =>
                        page.Questions.Contains(q) || model.PageOf(q).ShowWhen is not null
                    ),
                    page.Id
                );
            page.Questions.ShouldBeSubsetOf(model.AffectedBy(page), page.Id);
        }
    }

    [Fact]
    public void Model_HasThePatientAndClinicalRequirementsPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(
                    p.SectionId,
                    "patient-and-clinical-requirements",
                    StringComparison.Ordinal
                )
            )
            .Select(static p => p.Id)
            .ShouldBe([
                "screening-programme",
                "urgent-identification",
                "place-in-therapy",
                "treatment-duration",
                "nhs-service-changes",
                "handling-storage",
                "population-range",
                "eligible-population",
            ]);
    }

    [Fact]
    public void TriggerEntities_HoldTheEntitiesWithConditions()
    {
        Model
            .TriggerEntities.Select(static e => e.ClrType)
            .ShouldBe([
                typeof(MedicinesDevelopmentBackground),
                typeof(RecordHta),
                typeof(MedicinesEamsPim),
                typeof(MedicinesLaboratoryTesting),
                typeof(MedicinesPatientIdentification),
                typeof(MedicinesServiceImpact),
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
                "medicines_indication_detail.formulation_type_id",
                FormControl.Select,
                AnswerShape.Single
            ),
            ("medicines_indication_detail.presentation", FormControl.Textarea, AnswerShape.Single),
            (
                "medicines_indication_detail.mode_of_action",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail.proposed_dose_regimen",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail.is_personalised_medicine",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_indication_detail.medicine_technology_status",
                FormControl.Checkbox,
                AnswerShape.Multi
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
            ("record_hta.medicine_hta_submission_intended", FormControl.Radio, AnswerShape.Single),
            ("record_hta.medicine_hta_bodies", FormControl.Checkbox, AnswerShape.Multi),
            ("record_hta.hta_additional_details", FormControl.Textarea, AnswerShape.Single),
            ("record_hta.hta_nice_aligned_pathway", FormControl.Radio, AnswerShape.Single),
            ("record_hta.nice_ta_development_id", FormControl.PrefixedNumber, AnswerShape.Single),
            ("record_hta.uk_launch_date", FormControl.PrecisionDate, AnswerShape.Single),
            ("medicines_eams_pim.pim_designation_status", FormControl.Radio, AnswerShape.Single),
            ("medicines_eams_pim.will_submit_to_eams", FormControl.Radio, AnswerShape.Single),
            (
                "medicines_eams_pim.eams_submission_date",
                FormControl.PrecisionDate,
                AnswerShape.Single
            ),
            ("medicines_eams_pim.eams_opinion_date", FormControl.PrecisionDate, AnswerShape.Single),
            ("medicines_eams_pim.eams_opinion_decision", FormControl.Radio, AnswerShape.Single),
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
                "medicines_laboratory_testing.genomic_test_notes",
                FormControl.Textarea,
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
            (
                "medicines_patient_identification.screening_required",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_patient_identification.screening_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_patient_identification.urgent_identification_required",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_patient_identification.urgent_identification_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_treatment_detail.proposed_place_in_therapy",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_treatment_detail.estimated_duration_of_treatment",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.nhs_service_changes_required",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.nhs_service_changes_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.handling_storage_requirements",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.handling_storage_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.uk_patient_population_range_id",
                FormControl.Select,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.uk_patient_population_notes",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_service_impact.estimated_eligible_patient_population",
                FormControl.Textarea,
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
        string? Next(string pageId) => model.NextPage(model.FindPage(pageId)!, applicability)?.Id;
        string? Previous(string pageId) =>
            model.PreviousPage(model.FindPage(pageId)!, applicability)?.Id;

        model.PreviousPage(model.Pages[0], applicability).ShouldBeNull();
        Next("indication").ShouldBe("bnf-chapter");
        Previous("bnf-chapter").ShouldBe("indication");
        Next("rare-disease").ShouldBe("formulation");
        Next("technology-status").ShouldBeNull();
        Previous("repurposed-medicine").ShouldBeNull();
        Next("repurposed-medicine").ShouldBe("originator");
        Next("co-marketing").ShouldBeNull();
        Previous("hta-submission").ShouldBeNull();
        // With no answers the NICE pages do not apply, so the launch date follows the first page.
        Next("hta-submission").ShouldBe("uk-launch-date");
        Previous("uk-launch-date").ShouldBe("hta-submission");
        Next("uk-launch-date").ShouldBeNull();
        Previous("diagnostic-test-required").ShouldBeNull();
        // With no answers the conditional pages do not apply, so navigation skips to monitoring.
        Next("diagnostic-test-required").ShouldBe("monitoring-tests");
        Next("safety-tests").ShouldBeNull();
        Previous("screening-programme").ShouldBeNull();
        Next("screening-programme").ShouldBe("urgent-identification");
        Next("eligible-population").ShouldBeNull();
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
