using Shouldly;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Tests.Application.RecordForms;

public class MedicineFormModelTests
{
    // Bump FormVersions.Medicine whenever this changes, then update both values.
    private const string ExpectedVersion = "2026.11.23.4";
    private const string ExpectedFingerprint =
        "19dbe6fc32fc525e7acc06a4d19eda4ccf8be09fb6dfdfeed6afecdda069d153";

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
                string.Equals(p.TaskId, "indication-details", StringComparison.Ordinal)
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
                string.Equals(p.TaskId, "laboratory-testing", StringComparison.Ordinal)
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
                string.Equals(p.TaskId, "pim-and-eams", StringComparison.Ordinal)
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
                string.Equals(p.TaskId, "development-background", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe(["repurposed-medicine", "originator", "co-marketing"]);
    }

    [Fact]
    public void Model_HasTheHtaAndLaunchPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.TaskId, "hta-and-launch", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe(["hta-submission", "nice-aligned-pathway", "nice-ta-id", "uk-launch-date"]);
    }

    [Fact]
    public void Model_HasTheOrphanAndAtmpPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.TaskId, "orphan-and-atmp", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe(["orphan-status", "atmp-classification"]);
    }

    [Fact]
    public void Model_OrphanAndAtmpDatesAreActualDates()
    {
        Model
            .FindQuestion("medicines_eu_status.eu_orphan_granted_date")!
            .Control.ShouldBe(FormControl.ActualDate);
        Model
            .FindQuestion("medicines_eu_status.atmp_recommendation_date")!
            .Control.ShouldBe(FormControl.ActualDate);
    }

    [Fact]
    public void Model_AtmpStatusDoesNotOfferNoSubmissionIntended()
    {
        string orphan = _options.Find(OptionKeys.EuOrphanStatus)!.Description;
        string atmp = _options.Find(OptionKeys.EuAtmpClassificationStatus)!.Description;

        orphan.ShouldContain("NoSubmissionIntended");
        atmp.ShouldNotContain("NoSubmissionIntended");
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
                    "medicines_eu_status.eu_orphan_granted_date",
                    "AnswerIn medicines_eu_status.eu_orphan_status [Granted]"
                ),
                (
                    "medicines_eu_status.eu_orphan_status_number",
                    "AnswerIn medicines_eu_status.eu_orphan_status [Granted]"
                ),
                (
                    "medicines_eu_status.atmp_recommendation_date",
                    "AnswerIn medicines_eu_status.eu_atmp_classification_status [Granted]"
                ),
                (
                    "medicines_eu_status.atmp_classification_id",
                    "AnswerIn medicines_eu_status.eu_atmp_classification_status [Granted]"
                ),
                (
                    "medicines_eams_pim.will_submit_to_eams",
                    "AnswerIn medicines_eams_pim.pim_designation_status [Granted]"
                ),
                (
                    "medicines_laboratory_testing.genomic_test_pathway_point_other",
                    "AnswerIn medicines_laboratory_testing.patient_pathway_point_id [other]"
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
                (
                    "medicines_budget_impact.compassionate_access_details",
                    "AnswerIn medicines_budget_impact.compassionate_access_available [Yes]"
                ),
                (
                    "medicines_budget_impact.patient_access_scheme_regions",
                    "AnswerIn medicines_budget_impact.patient_access_scheme_planned [Yes]"
                ),
                (
                    "medicines_budget_impact.indication_specific_pricing_details",
                    "AnswerIn medicines_budget_impact.indication_specific_pricing_planned [Yes]"
                ),
            ]);
        Model
            .Pages.Where(static p => p.ShowWhen is not null)
            .Select(static p => p.Id)
            .ShouldBe([
                "clinical-trial-details",
                "recruiting-in-uk",
                "global-first-submission",
                "international-recognition",
                "international-submission-date",
                "international-licence-date",
                "international-conditional-approval",
                "nice-aligned-pathway",
                "nice-ta-id",
                "pim-designation",
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
    public void Model_HasTheMhraProcedureAndDatesPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.TaskId, "mhra-procedure-and-dates", StringComparison.Ordinal)
            )
            .Select(static p => (p.Id, p.ShowWhen?.Description))
            .ShouldBe([
                ("uk-market-authorisation", null),
                ("uk-submission-date", null),
                (
                    "global-first-submission",
                    "AnswerIn record_mhra_date.uk_submission_date [ActualDate]"
                ),
                ("uk-licence-date", null),
                ("uk-conditional-approval", null),
                (
                    "international-recognition",
                    "AnswerIn record_mhra_procedure.mhra_procedure_type_id [international-recognition]"
                ),
                (
                    "international-submission-date",
                    "AnswerIn record_mhra_procedure.mhra_procedure_type_id [international-recognition]"
                ),
                (
                    "international-licence-date",
                    "AnswerIn record_mhra_procedure.mhra_procedure_type_id [international-recognition]"
                ),
                (
                    "international-conditional-approval",
                    "AnswerIn record_mhra_procedure.mhra_procedure_type_id [international-recognition]"
                ),
            ]);
    }

    [Fact]
    public void Model_HasTheClinicalTrialsPagesInOrder()
    {
        FormTask task = Model
            .Sections.SelectMany(static s => s.Tasks)
            .Single(static t => string.Equals(t.Id, "clinical-trials", StringComparison.Ordinal));

        task.Pages.Select(static p => (p.Id, p.CommercialInConfidence))
            .ShouldBe([
                ("add-clinical-trial", false),
                ("clinical-trial-details", false),
                ("recruiting-in-uk", true),
            ]);
        task.Pages.Skip(1)
            .Select(static p => p.ShowWhen!.Description)
            .ShouldBe([
                "AnswerIn record_clinical_trial_information.has_clinical_trials [Yes]",
                "AnswerIn record_clinical_trial_information.has_clinical_trials [Yes]",
            ]);
        task.Pages[0].Questions.Single().OptionKey.ShouldBe(OptionKeys.YesNo);
        task.Pages[2]
            .Questions.Single()
            .OptionKey.ShouldBe(OptionKeys.ClinicalTrialsRecruitingInUk);
    }

    [Fact]
    public void Model_ClinicalTrialsAreAGroupWithANestedTextList()
    {
        FormQuestion trials = Model.FindQuestion("record_clinical_trial")!;

        (trials.ItemLabel, trials.AddAnotherLabel, trials.RemoveLabel, trials.ItemKey).ShouldBe(
            ("Clinical trial", "Add another clinical trial", "Remove clinical trial", null)
        );
        trials
            .Children.Select(static c => (c.Id, c.Control))
            .ShouldBe([
                ("study_name", FormControl.Text),
                ("clinical_trials_gov_number", FormControl.PrefixedNumber),
                ("other_registry_numbers", FormControl.TextList),
            ]);
        trials.Children[1].Prefixes.ShouldBe(["NCT"]);
        trials.Children[1].PrefixDigits.ShouldBe(8);
        trials.Children[2].ItemLabel.ShouldBe("Other registry number");
        trials
            .Children.Select(static c => string.Join(",", c.Rules.Select(static r => r.Kind)))
            .ShouldBe(["Required", "Required", "Distinct"]);
        trials.Entity.Includes.ShouldBe([
            "ClinicalTrials",
            "ClinicalTrials.OtherClinicalTrialNumbers",
        ]);
    }

    [Fact]
    public void Model_SectionsGroupTasksPerDataset()
    {
        Model
            .Sections.Select(static s =>
                $"{s.Id}: {string.Join(", ", s.Tasks.Select(static t => t.Id))}"
            )
            .ShouldBe([
                "product-record-details: names-and-identifiers",
                "indication-and-development: indication-details, development-background",
                "clinical-trial-information: clinical-trials",
                "regulatory-access-and-launch: mhra-procedure-and-dates, hta-and-launch",
                "special-designation: orphan-and-atmp, pim-and-eams",
                "service-readiness: laboratory-testing, patient-and-clinical-requirements, pricing-and-budget-impact",
            ]);
    }

    [Fact]
    public void ReferenceTriggerTypes_HoldTheReferenceDataConditionsName()
    {
        Model.ReferenceTriggerTypes.ShouldBe([
            typeof(MhraProcedureType),
            typeof(PatientPathwayPoint),
        ]);
    }

    [Fact]
    public void EveryPage_ReadsAndSavesOnlyItsOwnRows()
    {
        // Reveals are same-page, so they add no rows. Page conditions add the entities holding the
        // triggers to every request, which navigation needs to know which pages apply, and a save
        // adds the entities of the answers it may clear.
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
            model
                .EntitiesToSave(page)
                .ShouldBe(
                    expected.Union(model.AffectedBy(page).Select(static q => q.Entity)),
                    ignoreOrder: true,
                    page.Id
                );

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
                    p.TaskId,
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
    public void Model_HasThePricingAndBudgetImpactPagesInOrder()
    {
        Model
            .Pages.Where(static p =>
                string.Equals(p.TaskId, "pricing-and-budget-impact", StringComparison.Ordinal)
            )
            .Select(static p => p.Id)
            .ShouldBe([
                "estimated-uptake",
                "compassionate-access",
                "patient-access-scheme",
                "indication-specific-pricing",
                "net-budget-impact",
            ]);
    }

    [Fact]
    public void TriggerEntities_HoldTheEntitiesWithConditions()
    {
        Model
            .TriggerEntities.Select(static e => e.ClrType)
            .ShouldBe([
                typeof(MedicinesDevelopmentBackground),
                typeof(RecordClinicalTrialInformation),
                typeof(RecordMhraDate),
                typeof(RecordMhraProcedure),
                typeof(RecordHta),
                typeof(MedicinesEuStatus),
                typeof(MedicinesIndicationDetail),
                typeof(MedicinesEamsPim),
                typeof(MedicinesLaboratoryTesting),
                typeof(MedicinesPatientIdentification),
                typeof(MedicinesServiceImpact),
                typeof(MedicinesBudgetImpact),
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
            ("record_product_detail.company_code", FormControl.Text, AnswerShape.Single),
            ("record_product_detail.other_identifiers", FormControl.TextList, AnswerShape.List),
            ("record_product_detail.branded_name", FormControl.Text, AnswerShape.Single),
            ("record_product_detail.generic_names", FormControl.TextList, AnswerShape.List),
            ("record_product_detail.record_title", FormControl.Textarea, AnswerShape.Single),
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
            (
                "record_clinical_trial_information.has_clinical_trials",
                FormControl.Radio,
                AnswerShape.Single
            ),
            ("record_clinical_trial", FormControl.Group, AnswerShape.Group),
            (
                "record_clinical_trial_information.recruiting_in_uk",
                FormControl.Radio,
                AnswerShape.Single
            ),
            ("record_mhra_procedure.mhra_procedure_type_id", FormControl.Radio, AnswerShape.Single),
            ("record_mhra_procedure.procedure_details", FormControl.Textarea, AnswerShape.Single),
            ("record_mhra_date.uk_submission_date", FormControl.PrecisionDate, AnswerShape.Single),
            (
                "medicines_global_submission.global_first_submission_region",
                FormControl.Text,
                AnswerShape.Single
            ),
            (
                "medicines_global_submission.global_submission_actual_date",
                FormControl.ActualMonth,
                AnswerShape.Single
            ),
            ("record_mhra_date.uk_licence_date", FormControl.PrecisionDate, AnswerShape.Single),
            (
                "record_mhra_date.uk_conditional_approval_anticipated",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "record_mhra_procedure.irp_reference_regulator_id",
                FormControl.Radio,
                AnswerShape.Single
            ),
            ("medicines_intl_recognition.irp_route_id", FormControl.Radio, AnswerShape.Single),
            (
                "medicines_intl_recognition.intl_submission_date",
                FormControl.PrecisionDate,
                AnswerShape.Single
            ),
            (
                "medicines_intl_recognition.intl_licence_date",
                FormControl.PrecisionDate,
                AnswerShape.Single
            ),
            (
                "medicines_intl_recognition.intl_conditional_approval_anticipated",
                FormControl.Radio,
                AnswerShape.Single
            ),
            ("record_hta.medicine_hta_submission_intended", FormControl.Radio, AnswerShape.Single),
            ("record_hta.medicine_hta_bodies", FormControl.Checkbox, AnswerShape.Multi),
            ("record_hta.hta_additional_details", FormControl.Textarea, AnswerShape.Single),
            ("record_hta.hta_nice_aligned_pathway", FormControl.Radio, AnswerShape.Single),
            ("record_hta.nice_ta_development_id", FormControl.PrefixedNumber, AnswerShape.Single),
            ("record_hta.uk_launch_date", FormControl.PrecisionDate, AnswerShape.Single),
            ("medicines_eu_status.eu_orphan_status", FormControl.Radio, AnswerShape.Single),
            (
                "medicines_eu_status.eu_orphan_granted_date",
                FormControl.ActualDate,
                AnswerShape.Single
            ),
            ("medicines_eu_status.eu_orphan_status_number", FormControl.Text, AnswerShape.Single),
            (
                "medicines_eu_status.eu_atmp_classification_status",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_eu_status.atmp_recommendation_date",
                FormControl.ActualDate,
                AnswerShape.Single
            ),
            ("medicines_eu_status.atmp_classification_id", FormControl.Radio, AnswerShape.Single),
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
                "medicines_laboratory_testing.genomic_test_pathway_point_other",
                FormControl.Textarea,
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
            ("medicines_budget_impact.estimated_uptake", FormControl.Textarea, AnswerShape.Single),
            (
                "medicines_budget_impact.compassionate_access_available",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_budget_impact.compassionate_access_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_budget_impact.patient_access_scheme_planned",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_budget_impact.patient_access_scheme_regions",
                FormControl.Checkbox,
                AnswerShape.Multi
            ),
            (
                "medicines_budget_impact.indication_specific_pricing_planned",
                FormControl.Radio,
                AnswerShape.Single
            ),
            (
                "medicines_budget_impact.indication_specific_pricing_details",
                FormControl.Textarea,
                AnswerShape.Single
            ),
            (
                "medicines_budget_impact.net_uk_budget_impact_band",
                FormControl.Radio,
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
        Previous("uk-market-authorisation").ShouldBeNull();
        // With no answers the global first submission and IRP pages do not apply.
        Next("uk-submission-date").ShouldBe("uk-licence-date");
        Next("uk-conditional-approval").ShouldBeNull();
        Previous("hta-submission").ShouldBeNull();
        // With no answers the NICE pages do not apply, so the launch date follows the first page.
        Next("hta-submission").ShouldBe("uk-launch-date");
        Previous("uk-launch-date").ShouldBe("hta-submission");
        Next("uk-launch-date").ShouldBeNull();
        Previous("orphan-status").ShouldBeNull();
        Next("orphan-status").ShouldBe("atmp-classification");
        Previous("atmp-classification").ShouldBe("orphan-status");
        Next("atmp-classification").ShouldBeNull();
        Previous("diagnostic-test-required").ShouldBeNull();
        // With no answers the conditional pages do not apply, so navigation skips to monitoring.
        Next("diagnostic-test-required").ShouldBe("monitoring-tests");
        Next("safety-tests").ShouldBeNull();
        Previous("screening-programme").ShouldBeNull();
        Next("screening-programme").ShouldBe("urgent-identification");
        Next("eligible-population").ShouldBeNull();
        Previous("estimated-uptake").ShouldBeNull();
        Next("estimated-uptake").ShouldBe("compassionate-access");
        Next("net-budget-impact").ShouldBeNull();
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
