using System.Globalization;
using UKPS.Api.Persistence.Enums;
using static UKPS.Api.Persistence.Data.Seeding.SyntheticData.SyntheticRecord;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// Works out what a synthetic record said in its earlier revisions, and which tracked fields
/// changed between revisions.
/// </summary>
internal static class RecordContentHistory
{
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

    /// <summary>The fields whose changes are recorded against published revisions.</summary>
    public static IReadOnlyDictionary<string, string?> TrackedFields(SyntheticRecord record)
    {
        SyntheticMhraProcedureAndDates mhra = record
            .RegulatoryAccessAndLaunchInformation
            .MhraProcedureAndDates;
        SyntheticHtaAndLaunch hta = record
            .RegulatoryAccessAndLaunchInformation
            .HealthTechnologyAssessmentAndLaunch;
        SyntheticSpecialDesignations designations = record
            .RegulatoryAccessAndLaunchInformation
            .SpecialDesignations;

        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["record_mhra_date.uk_submission_date"] = Format(mhra.UkSubmissionDate),
            ["record_mhra_date.uk_licence_date"] = Format(mhra.UkLicenceDate),
            ["record_mhra_date.uk_conditional_approval_anticipated"] =
                mhra.UkConditionalApprovalAnticipated?.ToString(),
            ["medicines_global_submission.global_submission_actual_date"] = Format(
                mhra.GlobalSubmissionActualDate
            ),
            ["medicines_intl_recognition.intl_submission_date"] = Format(mhra.IntlSubmissionDate),
            ["medicines_intl_recognition.intl_licence_date"] = Format(mhra.IntlLicenceDate),
            ["medicines_intl_recognition.intl_conditional_approval_anticipated"] =
                mhra.IntlConditionalApprovalAnticipated?.ToString(),
            ["record_hta.uk_launch_date"] = Format(hta.UkLaunchDate),
            ["record_hta.hta_nice_aligned_pathway"] = hta.HtaNiceAlignedPathway?.ToString(),
            ["medicines_eu_status.eu_orphan_status"] = designations.EuOrphanStatus?.ToString(),
            ["medicines_eu_status.eu_orphan_granted_date"] = Format(
                designations.EuOrphanGrantedDate
            ),
            ["medicines_eu_status.eu_orphan_status_number"] = designations.EuOrphanStatusNumber,
            ["medicines_eu_status.eu_atmp_classification_status"] =
                designations.EuAtmpClassificationStatus?.ToString(),
            ["medicines_eu_status.atmp_recommendation_date"] = Format(
                designations.AtmpRecommendationDate
            ),
            ["medicines_eu_status.atmp_classification"] = designations.AtmpClassification,
            ["medicines_eams_pim.eams_submission_date"] = Format(designations.EamsSubmissionDate),
            ["medicines_eams_pim.eams_opinion_date"] = Format(designations.EamsOpinionDate),
            ["medicines_eams_pim.eams_opinion_decision"] =
                designations.EamsOpinionDecision?.ToString(),
            ["medicines_indication_detail.indication_is_rare_disease"] =
                record.IndicationAndDevelopmentInformation.IndicationDetails.IndicationIsRareDisease?.ToString(),
            ["record_clinical_trial_information.recruiting_in_uk"] =
                record.ClinicalTrialInformation.RecruitingInUk?.ToString(),
        };
    }

    /// <summary>The tracked fields that differ between two versions of a record.</summary>
    public static IReadOnlyList<(string FieldPath, string? OldValue, string? NewValue)> Changes(
        SyntheticRecord previous,
        SyntheticRecord current
    )
    {
        IReadOnlyDictionary<string, string?> before = TrackedFields(previous);
        return
        [
            .. TrackedFields(current)
                .Where(field =>
                    !string.Equals(before[field.Key], field.Value, StringComparison.Ordinal)
                )
                .Select(field => (field.Key, before[field.Key], field.Value)),
        ];
    }

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
}
