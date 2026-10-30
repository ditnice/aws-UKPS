using System.Globalization;
using System.Text.Json;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using static UKPS.Api.Persistence.Data.Seeding.SyntheticData.SyntheticRecord;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// Works out what a synthetic record said in its earlier revisions and drafts, and which audited
/// fields changed between them.
/// </summary>
internal static class RecordContentHistory
{
    /// <summary>Phrases that introduce the population an indication applies to.</summary>
    private static readonly string[] _populationPhrases =
    [
        " in adult",
        " in patients",
        " whose ",
        " who ",
    ];

    /// <summary>Yes/No answers that can start out as Unknown and be disclosed in a later revision.</summary>
    public static readonly IReadOnlyList<string> DisclosableFields =
    [
        "record_mhra_date.uk_conditional_approval_anticipated",
        "medicines_intl_recognition.intl_conditional_approval_anticipated",
        "medicines_indication_detail.indication_is_rare_disease",
        "record_hta.hta_nice_aligned_pathway",
        "record_clinical_trial_information.recruiting_in_uk",
    ];

    /// <summary>
    /// Returns the record as it would have been known on <paramref name="asOf"/>.
    /// </summary>
    /// <param name="record">The record's latest content.</param>
    /// <param name="asOf">The date the earlier revision was published.</param>
    /// <param name="estimateDriftMonths">
    /// How many months earlier the estimates were at the time, as timelines tend to slip.
    /// </param>
    /// <param name="unknownFields">Disclosable fields that were still Unknown at the time.</param>
    public static SyntheticRecord AsKnownAt(
        SyntheticRecord record,
        DateOnly asOf,
        int estimateDriftMonths,
        IReadOnlySet<string> unknownFields
    )
    {
        DateEstimator dates = new(asOf, estimateDriftMonths);
        Disclosure disclosure = new(unknownFields);
        SyntheticIndicationDetails indication = record
            .IndicationAndDevelopmentInformation
            .IndicationDetails;

        return record with
        {
            IndicationAndDevelopmentInformation = record.IndicationAndDevelopmentInformation with
            {
                IndicationDetails = indication with
                {
                    IndicationIsRareDisease = disclosure.Apply(
                        "medicines_indication_detail.indication_is_rare_disease",
                        indication.IndicationIsRareDisease
                    ),
                },
            },
            ClinicalTrialInformation = record.ClinicalTrialInformation with
            {
                RecruitingInUk = disclosure.Apply(
                    "record_clinical_trial_information.recruiting_in_uk",
                    record.ClinicalTrialInformation.RecruitingInUk
                ),
            },
            RegulatoryAccessAndLaunchInformation = RegulatoryAsKnownAt(
                record.RegulatoryAccessAndLaunchInformation,
                dates,
                disclosure
            ),
        };
    }

    private static SyntheticRegulatoryAccessAndLaunch RegulatoryAsKnownAt(
        SyntheticRegulatoryAccessAndLaunch regulatory,
        DateEstimator dates,
        Disclosure disclosure
    )
    {
        SyntheticMhraProcedureAndDates mhra = regulatory.MhraProcedureAndDates;
        SyntheticHtaAndLaunch hta = regulatory.HealthTechnologyAssessmentAndLaunch;
        return regulatory with
        {
            MhraProcedureAndDates = mhra with
            {
                UkSubmissionDate = dates.Estimate(mhra.UkSubmissionDate),
                UkLicenceDate = dates.Estimate(mhra.UkLicenceDate),
                UkConditionalApprovalAnticipated = disclosure.Apply(
                    "record_mhra_date.uk_conditional_approval_anticipated",
                    mhra.UkConditionalApprovalAnticipated
                ),
                GlobalSubmissionActualDate = dates.ActualOnly(mhra.GlobalSubmissionActualDate),
                IntlSubmissionDate = dates.Estimate(mhra.IntlSubmissionDate),
                IntlLicenceDate = dates.Estimate(mhra.IntlLicenceDate),
                IntlConditionalApprovalAnticipated = disclosure.Apply(
                    "medicines_intl_recognition.intl_conditional_approval_anticipated",
                    mhra.IntlConditionalApprovalAnticipated
                ),
            },
            HealthTechnologyAssessmentAndLaunch = hta with
            {
                UkLaunchDate = dates.Estimate(hta.UkLaunchDate),
                HtaNiceAlignedPathway = disclosure.Apply(
                    "record_hta.hta_nice_aligned_pathway",
                    hta.HtaNiceAlignedPathway
                ),
            },
            SpecialDesignations = SpecialDesignationsAsKnownAt(
                regulatory.SpecialDesignations,
                dates
            ),
        };
    }

    /// <summary>
    /// The fields whose changes are recorded in the field audit, in form order. Reference data
    /// answers hold the synthetic code or label until <see cref="SeedReferenceData.ResolveAuditValue"/>
    /// swaps it for the ID, once the reference data is saved.
    /// </summary>
    public static IReadOnlyDictionary<string, string?> AuditedFields(SyntheticRecord record)
    {
        SyntheticRegulatoryAccessAndLaunch regulatory = record.RegulatoryAccessAndLaunchInformation;
        return new[]
        {
            CreationPageFields(record.ProductRecordDetails.NamesAndIdentifiers),
            IndicationFields(record),
            MhraFields(regulatory.MhraProcedureAndDates),
            HtaAndDesignationFields(regulatory),
            ServiceReadinessFields(record.ServiceReadinessInformation),
        }
            .SelectMany(fields => fields)
            .ToDictionary(field => field.Path, field => field.Value, StringComparer.Ordinal);
    }

    /// <summary>The fields answered on the page that creates the record, saved with the record.</summary>
    public static IReadOnlyList<(string Path, string? Value)> CreationPageFields(
        SyntheticNamesAndIdentifiers names
    ) =>
        [
            ("record_product_detail.company_code", names.CompanyCode),
            ("record_product_detail.branded_name", names.BrandedName),
            ("record_product_detail.record_title", names.RecordTitle),
            (
                "record_name_and_identifier",
                RecordNameAndIdentifier.AuditText(names.GenericNames, names.OtherIdentifiers)
            ),
        ];

    private static (string Path, string? Value)[] IndicationFields(SyntheticRecord record)
    {
        SyntheticIndicationDetails indication = record
            .IndicationAndDevelopmentInformation
            .IndicationDetails;
        return
        [
            ("medicines_indication_detail.indication", indication.Indication),
            ("medicines_indication_detail.bnf_chapter_id", indication.BnfChapter?.Code),
            (
                "medicines_indication_detail_therapeutic_area",
                indication.TherapeuticAreas.Count == 0
                    ? null
                    : JsonSerializer.Serialize(
                        indication
                            .TherapeuticAreas.Select(area => area.Code)
                            .Order(StringComparer.Ordinal)
                    )
            ),
            (
                "medicines_indication_detail.indication_is_paediatric",
                indication.IndicationIsPaediatric?.ToString()
            ),
            (
                "medicines_indication_detail.indication_is_cancer",
                indication.IndicationIsCancer?.ToString()
            ),
            (
                "medicines_indication_detail.indication_is_rare_disease",
                indication.IndicationIsRareDisease?.ToString()
            ),
            ("medicines_indication_detail.formulation_type_id", indication.FormulationType),
            ("medicines_indication_detail.presentation", indication.Presentation),
            ("medicines_indication_detail.mode_of_action", indication.ModeOfAction),
            ("medicines_indication_detail.proposed_dose_regimen", indication.ProposedDoseRegimen),
            (
                "medicines_indication_detail.is_personalised_medicine",
                indication.IsPersonalisedMedicine?.ToString()
            ),
            (
                "record_clinical_trial_information.recruiting_in_uk",
                record.ClinicalTrialInformation.RecruitingInUk?.ToString()
            ),
        ];
    }

    private static (string Path, string? Value)[] MhraFields(SyntheticMhraProcedureAndDates mhra) =>
        [
            ("record_mhra_procedure.mhra_procedure_type_id", mhra.MhraProcedureType),
            ("record_mhra_procedure.irp_reference_regulator_id", mhra.IrpReferenceRegulator),
            ("record_mhra_date.uk_submission_date", Format(mhra.UkSubmissionDate)),
            ("record_mhra_date.uk_licence_date", Format(mhra.UkLicenceDate)),
            (
                "record_mhra_date.uk_conditional_approval_anticipated",
                mhra.UkConditionalApprovalAnticipated?.ToString()
            ),
            (
                "medicines_global_submission.global_first_submission_region",
                mhra.GlobalFirstSubmissionRegion
            ),
            (
                "medicines_global_submission.global_submission_actual_date",
                Format(mhra.GlobalSubmissionActualDate)
            ),
            ("medicines_intl_recognition.irp_route_id", mhra.IrpRoute),
            ("medicines_intl_recognition.intl_submission_date", Format(mhra.IntlSubmissionDate)),
            ("medicines_intl_recognition.intl_licence_date", Format(mhra.IntlLicenceDate)),
            (
                "medicines_intl_recognition.intl_conditional_approval_anticipated",
                mhra.IntlConditionalApprovalAnticipated?.ToString()
            ),
        ];

    private static (string Path, string? Value)[] HtaAndDesignationFields(
        SyntheticRegulatoryAccessAndLaunch regulatory
    )
    {
        SyntheticHtaAndLaunch hta = regulatory.HealthTechnologyAssessmentAndLaunch;
        SyntheticSpecialDesignations designations = regulatory.SpecialDesignations;
        return
        [
            (
                "record_hta.medicine_hta_submission_intended",
                hta.MedicineHtaSubmissionIntended?.ToString()
            ),
            ("record_hta.hta_nice_aligned_pathway", hta.HtaNiceAlignedPathway?.ToString()),
            ("record_hta.nice_ta_development_id", hta.NiceTaDevelopmentId),
            ("record_hta.uk_launch_date", Format(hta.UkLaunchDate)),
            ("medicines_eu_status.eu_orphan_status", designations.EuOrphanStatus?.ToString()),
            (
                "medicines_eu_status.eu_orphan_granted_date",
                Format(designations.EuOrphanGrantedDate)
            ),
            ("medicines_eu_status.eu_orphan_status_number", designations.EuOrphanStatusNumber),
            (
                "medicines_eu_status.eu_atmp_classification_status",
                designations.EuAtmpClassificationStatus?.ToString()
            ),
            (
                "medicines_eu_status.atmp_recommendation_date",
                Format(designations.AtmpRecommendationDate)
            ),
            ("medicines_eu_status.atmp_classification_id", designations.AtmpClassification),
            (
                "medicines_eams_pim.pim_designation_status",
                designations.PimDesignationStatus?.ToString()
            ),
            ("medicines_eams_pim.will_submit_to_eams", designations.WillSubmitToEams?.ToString()),
            ("medicines_eams_pim.eams_submission_date", Format(designations.EamsSubmissionDate)),
            ("medicines_eams_pim.eams_opinion_date", Format(designations.EamsOpinionDate)),
            (
                "medicines_eams_pim.eams_opinion_decision",
                designations.EamsOpinionDecision?.ToString()
            ),
        ];
    }

    private static (string Path, string? Value)[] ServiceReadinessFields(
        SyntheticServiceReadiness readiness
    ) =>
        [
            (
                "medicines_laboratory_testing.diagnostic_test_required",
                readiness.LaboratoryTestingDetails.DiagnosticTestRequired?.ToString()
            ),
            (
                "medicines_treatment_detail.proposed_place_in_therapy",
                readiness.PatientAndClinicalRequirements.ProposedPlaceInTherapy
            ),
            (
                "medicines_service_impact.uk_patient_population_range_id",
                readiness.PatientAndClinicalRequirements.UkPatientPopulationRange
            ),
            (
                "medicines_budget_impact.net_uk_budget_impact_band",
                readiness.PricingAndBudgetImpact.NetUkBudgetImpactBand?.ToString()
            ),
        ];

    /// <summary>Whether any audited field differs between two versions of a record.</summary>
    public static bool HasChanges(SyntheticRecord previous, SyntheticRecord current)
    {
        IReadOnlyDictionary<string, string?> before = AuditedFields(previous);
        return AuditedFields(current)
            .Any(field => !string.Equals(before[field.Key], field.Value, StringComparison.Ordinal));
    }

    /// <summary>
    /// Returns the record with its estimated UK dates three months later, or null if it has no
    /// estimated UK dates.
    /// </summary>
    public static SyntheticRecord? WithEstimatedUkDatesPushedBack(SyntheticRecord record)
    {
        SyntheticRegulatoryAccessAndLaunch regulatory = record.RegulatoryAccessAndLaunchInformation;
        SyntheticMhraProcedureAndDates mhra = regulatory.MhraProcedureAndDates;
        SyntheticHtaAndLaunch hta = regulatory.HealthTechnologyAssessmentAndLaunch;
        SyntheticRecord pushedBack = record with
        {
            RegulatoryAccessAndLaunchInformation = regulatory with
            {
                MhraProcedureAndDates = mhra with
                {
                    UkSubmissionDate = PushBackIfEstimated(mhra.UkSubmissionDate),
                    UkLicenceDate = PushBackIfEstimated(mhra.UkLicenceDate),
                },
                HealthTechnologyAssessmentAndLaunch = hta with
                {
                    UkLaunchDate = PushBackIfEstimated(hta.UkLaunchDate),
                },
            },
        };
        return HasChanges(record, pushedBack) ? pushedBack : null;

        static SyntheticRegulatoryDate? PushBackIfEstimated(SyntheticRegulatoryDate? date) =>
            date is null || date.DatePrecision == DatePrecision.ActualDate
                ? date
                : date with
                {
                    DateValue = date.DateValue.AddMonths(3),
                };
    }

    /// <summary>Mistakes QA sends back, each with the record as it was submitted with the mistake.</summary>
    public static IReadOnlyList<SubmissionFlaw> SubmissionFlaws { get; } =
    [
        new(
            "Please confirm the UK submission date and whether it is an estimate.",
            MisstatedSubmissionDatePrecision
        ),
        new(
            "The indication wording does not match the clinical trial population. Please review.",
            WithoutIndicationPopulation
        ),
        new(
            "Please add the reference regulator for the International Recognition Procedure.",
            record =>
                record
                    .RegulatoryAccessAndLaunchInformation
                    .MhraProcedureAndDates
                    .IrpReferenceRegulator
                    is null
                    ? null
                    : WithMhra(record, mhra => mhra with { IrpReferenceRegulator = null })
        ),
        new("Proposed place in therapy needs likely comparators.", WithoutComparators),
        new(
            "Please add the BNF chapter.",
            record =>
                record.IndicationAndDevelopmentInformation.IndicationDetails.BnfChapter is null
                    ? null
                    : WithIndication(record, indication => indication with { BnfChapter = null })
        ),
    ];

    private static SyntheticRecord? MisstatedSubmissionDatePrecision(SyntheticRecord record)
    {
        SyntheticRegulatoryDate? date = record
            .RegulatoryAccessAndLaunchInformation
            .MhraProcedureAndDates
            .UkSubmissionDate;
        if (date is null)
        {
            return null;
        }

        SyntheticRegulatoryDate misstated =
            date.DatePrecision == DatePrecision.ActualDate
                ? date with
                {
                    DateValue = new DateOnly(date.DateValue.Year, date.DateValue.Month, 1),
                    DatePrecision = DatePrecision.EstimatedMonth,
                }
                : date with
                {
                    DatePrecision = DatePrecision.ActualDate,
                };
        return WithMhra(record, mhra => mhra with { UkSubmissionDate = misstated });
    }

    private static SyntheticRecord? WithoutIndicationPopulation(SyntheticRecord record)
    {
        string? indication = record
            .IndicationAndDevelopmentInformation
            .IndicationDetails
            .Indication;
        int populationStart = _populationPhrases
            .Select(phrase => indication?.IndexOf(phrase, StringComparison.Ordinal) ?? -1)
            .Where(index => index > 0)
            .DefaultIfEmpty(-1)
            .Min();
        return populationStart < 0
            ? null
            : WithIndication(
                record,
                details => details with { Indication = indication![..populationStart] + "." }
            );
    }

    private static SyntheticRecord? WithoutComparators(SyntheticRecord record)
    {
        SyntheticPatientAndClinicalRequirements patient = record
            .ServiceReadinessInformation
            .PatientAndClinicalRequirements;
        int comparators =
            patient.ProposedPlaceInTherapy?.IndexOf(
                " | Likely comparators",
                StringComparison.Ordinal
            ) ?? -1;
        if (comparators < 0)
        {
            return null;
        }

        return record with
        {
            ServiceReadinessInformation = record.ServiceReadinessInformation with
            {
                PatientAndClinicalRequirements = patient with
                {
                    ProposedPlaceInTherapy = patient.ProposedPlaceInTherapy![..comparators],
                },
            },
        };
    }

    private static SyntheticRecord WithMhra(
        SyntheticRecord record,
        Func<SyntheticMhraProcedureAndDates, SyntheticMhraProcedureAndDates> change
    ) =>
        record with
        {
            RegulatoryAccessAndLaunchInformation = record.RegulatoryAccessAndLaunchInformation with
            {
                MhraProcedureAndDates = change(
                    record.RegulatoryAccessAndLaunchInformation.MhraProcedureAndDates
                ),
            },
        };

    private static SyntheticRecord WithIndication(
        SyntheticRecord record,
        Func<SyntheticIndicationDetails, SyntheticIndicationDetails> change
    ) =>
        record with
        {
            IndicationAndDevelopmentInformation = record.IndicationAndDevelopmentInformation with
            {
                IndicationDetails = change(
                    record.IndicationAndDevelopmentInformation.IndicationDetails
                ),
            },
        };

    private static SyntheticSpecialDesignations SpecialDesignationsAsKnownAt(
        SyntheticSpecialDesignations designations,
        DateEstimator dates
    )
    {
        SyntheticSpecialDesignations result = designations with
        {
            EamsSubmissionDate = dates.Estimate(designations.EamsSubmissionDate),
            EamsOpinionDate = dates.Estimate(designations.EamsOpinionDate),
        };

        // A decision is only known once the opinion has been given.
        if (result.EamsOpinionDate?.DatePrecision != DatePrecision.ActualDate)
        {
            result = result with { EamsOpinionDecision = null };
        }

        // Designations granted later were still awaiting a decision.
        if (dates.IsFuture(designations.EuOrphanGrantedDate))
        {
            result = result with
            {
                EuOrphanStatus = DesignationStatus.ApplicationSubmittedDecisionPending,
                EuOrphanGrantedDate = null,
                EuOrphanStatusNumber = null,
            };
        }
        if (dates.IsFuture(designations.AtmpRecommendationDate))
        {
            result = result with
            {
                EuAtmpClassificationStatus = DesignationStatus.ApplicationSubmittedDecisionPending,
                AtmpRecommendationDate = null,
                AtmpClassification = null,
            };
        }
        return result;
    }

    private static string? Format(SyntheticRegulatoryDate? date) =>
        date is null
            ? null
            : string.Create(
                CultureInfo.InvariantCulture,
                $"{date.DateValue:yyyy-MM-dd} ({date.DatePrecision})"
            );

    private sealed class Disclosure(IReadOnlySet<string> unknownFields)
    {
        public YesNoUnknown? Apply(string field, YesNoUnknown? value) =>
            value is not null && unknownFields.Contains(field) ? YesNoUnknown.Unknown : value;
    }

    private sealed class DateEstimator(DateOnly asOf, int driftMonths)
    {
        private readonly DateOnly _earliestEstimate = new DateOnly(
            asOf.Year,
            asOf.Month,
            1
        ).AddMonths(1);

        public bool IsFuture(SyntheticRegulatoryDate? date) =>
            date is not null && date.DateValue > asOf;

        /// <summary>Dates that had not happened yet were estimates, made before any later slippage.</summary>
        public SyntheticRegulatoryDate? Estimate(SyntheticRegulatoryDate? date)
        {
            bool happened =
                date?.DatePrecision == DatePrecision.ActualDate && date.DateValue <= asOf;
            if (
                date is null
                || happened
                || (driftMonths == 0 && date.DatePrecision != DatePrecision.ActualDate)
            )
            {
                return date;
            }

            DateOnly estimate = date.DateValue.AddMonths(-driftMonths);
            estimate = new DateOnly(estimate.Year, estimate.Month, 1);
            if (estimate < _earliestEstimate)
            {
                estimate = _earliestEstimate;
            }

            if (date.DatePrecision == DatePrecision.EstimatedQuarter)
            {
                int quarterStartMonth = (3 * ((estimate.Month - 1) / 3)) + 1;
                estimate = new DateOnly(estimate.Year, quarterStartMonth, 1);
                return date with { DateValue = estimate };
            }

            return date with
            {
                DateValue = estimate,
                DatePrecision = DatePrecision.EstimatedMonth,
            };
        }

        /// <summary>Dates that are only recorded once they happen were blank beforehand.</summary>
        public SyntheticRegulatoryDate? ActualOnly(SyntheticRegulatoryDate? date) =>
            IsFuture(date) ? null : date;
    }

    /// <summary>A mistake QA sends back for correction.</summary>
    /// <param name="Note">QA's rejection note.</param>
    /// <param name="Introduce">
    /// Returns the record as submitted with the mistake, or null if it cannot apply to the record.
    /// </param>
    internal sealed record SubmissionFlaw(
        string Note,
        Func<SyntheticRecord, SyntheticRecord?> Introduce
    );
}
