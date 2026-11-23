using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Holds the PIM designation and EAMS answers, and owns the EamsSubmission and EamsOpinion
/// regulatory date rows.
/// </summary>
[FormEntity]
[FormPageShowWhen(
    MedicineFormLayout.PimDesignation,
    "medicines_indication_detail.medicine_technology_status",
    MedicineTechnologyStatus.Biosimilar,
    NoneOf = true
)]
[FormPageShowWhen(
    MedicineFormLayout.EamsSubmissionDate,
    "medicines_eams_pim.will_submit_to_eams",
    YesNoUnknown.Yes
)]
[FormPageShowWhen(
    MedicineFormLayout.EamsOpinionDate,
    "medicines_eams_pim.will_submit_to_eams",
    YesNoUnknown.Yes
)]
[FormPageShowWhen(
    MedicineFormLayout.EamsOpinionDecision,
    "medicines_eams_pim.eams_opinion_date",
    DatePrecision.ActualDate
)]
internal sealed class MedicinesEamsPim
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormPage(MedicineFormLayout.PimDesignation)]
    [FormField(
        "medicines_eams_pim.pim_designation_status",
        "Do you have, or are you seeking, Medicines and Healthcare Regulatory Agency (MHRA) PIM designation?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select the current status of PIM designation for the indication in scope. Granted should only be selected once formally awarded by MHRA. If application is planned or under review, select the relevant pending option."
    )]
    [FormOptions(OptionKeys.PimDesignationStatus)]
    [FormRequired("Select an option")]
    public DesignationStatus? PimDesignationStatus { get; set; }

    /// <summary>Conditional on PimDesignationStatus = Granted.</summary>
    [FormPage(MedicineFormLayout.PimDesignation)]
    [FormField(
        "medicines_eams_pim.will_submit_to_eams",
        "Have you already or do you plan to submit to EAMS?",
        FormControl.Radio,
        Order = 20,
        Hint = "Select yes if you intend to pursue EAMS. Select no if you do not. Select unknown if undecided."
    )]
    [FormShowWhen("medicines_eams_pim.pim_designation_status", DesignationStatus.Granted)]
    [FormRequired("Select an option")]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? WillSubmitToEams { get; set; }

    [FormPage(MedicineFormLayout.EamsSubmissionDate)]
    [FormField(
        "medicines_eams_pim.eams_submission_date",
        "What is the EAMS submission date?",
        FormControl.PrecisionDate,
        Order = 10,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect to submit for EAMS for this product, or enter the actual date of submission."
    )]
    [FormDateEvent(DateEventType.EamsSubmission, Name = "EAMS submission")]
    [FormRequired("Select an option")]
    public int? EamsSubmissionDateId { get; set; }

    [FormPage(MedicineFormLayout.EamsOpinionDate)]
    [FormField(
        "medicines_eams_pim.eams_opinion_date",
        "Date of EAMS scientific opinion",
        FormControl.PrecisionDate,
        Order = 10,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect to receive the EAMS scientific opinion, or enter the actual date you received it."
    )]
    [FormDateEvent(DateEventType.EamsOpinion, Name = "EAMS scientific opinion")]
    [FormRequired("Select an option")]
    public int? EamsOpinionDateId { get; set; }

    [FormPage(MedicineFormLayout.EamsOpinionDecision)]
    [FormField(
        "medicines_eams_pim.eams_opinion_decision",
        "What was the EAMS scientific opinion decision?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select an option")]
    public EamsOpinionDecision? EamsOpinionDecision { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsOpinionDate { get; set; }
}
