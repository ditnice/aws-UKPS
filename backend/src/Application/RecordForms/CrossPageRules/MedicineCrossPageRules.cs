using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.CrossPageRules;

/// <summary>
/// The medicine form's rules between dates on different pages, from the medicines dataset. Each
/// date's own rules (real, in the past or future) are page rules.
/// </summary>
internal static class MedicineCrossPageRules
{
    private const string UkSubmission = "record_mhra_date.uk_submission_date";
    private const string UkLicence = "record_mhra_date.uk_licence_date";
    private const string UkLaunch = "record_hta.uk_launch_date";
    private const string IntlSubmission = "medicines_intl_recognition.intl_submission_date";
    private const string IntlLicence = "medicines_intl_recognition.intl_licence_date";
    private const string GlobalFirstSubmission =
        "medicines_global_submission.global_submission_actual_date";
    private const string EamsSubmission = "medicines_eams_pim.eams_submission_date";
    private const string EamsOpinion = "medicines_eams_pim.eams_opinion_date";
    private const string WillSubmitToEams = "medicines_eams_pim.will_submit_to_eams";

    public static IReadOnlyList<CrossPageRule> All { get; } =
    [
        new DateOrderRule(
            "CR-01",
            UkLicence,
            UkLaunch,
            AllowSame: true,
            "UK licence date must be on or before the UK commercial launch date",
            "UK commercial launch date must be on or after the UK licence date",
            ExemptWhen: new AnswerInCondition(WillSubmitToEams, [nameof(YesNoUnknown.Yes)])
        ),
        new DateOrderRule(
            "CR-02",
            UkSubmission,
            UkLicence,
            AllowSame: false,
            "UK regulatory submission date must be before the UK licence date",
            "UK licence date must be after the UK regulatory submission date"
        ),
        new DateOrderRule(
            "CR-03",
            UkSubmission,
            UkLaunch,
            AllowSame: false,
            "UK regulatory submission date must be before the UK commercial launch date",
            "UK commercial launch date must be after the UK regulatory submission date"
        ),
        new DateOrderRule(
            "CR-04",
            IntlSubmission,
            IntlLicence,
            AllowSame: false,
            "International regulatory submission date must be before the international licence date",
            "International licence date must be after the international regulatory submission date"
        ),
        new DateOrderRule(
            "CR-05",
            GlobalFirstSubmission,
            UkSubmission,
            AllowSame: true,
            "Global first submission date must be on or before the UK regulatory submission date",
            "UK regulatory submission date must be on or after the global first submission date"
        ),
        new DateOrderRule(
            "CR-06",
            GlobalFirstSubmission,
            IntlSubmission,
            AllowSame: true,
            "Global first submission date must be on or before the international regulatory submission date",
            "International regulatory submission date must be on or after the global first submission date"
        ),
        new DateOrderRule(
            "CR-07",
            EamsSubmission,
            EamsOpinion,
            AllowSame: false,
            "EAMS submission date must be before the EAMS scientific opinion date",
            "EAMS scientific opinion date must be after the EAMS submission date"
        ),
    ];
}
