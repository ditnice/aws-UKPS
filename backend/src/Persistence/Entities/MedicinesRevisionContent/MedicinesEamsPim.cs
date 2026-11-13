using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Holds the PIM designation and EAMS answers, and owns the EamsSubmission and EamsOpinion
/// regulatory date rows.
/// </summary>
[FormSection(
    RecordType.Medicine,
    "pim-and-eams",
    "Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)",
    Order = 60
)]
[FormPageShowWhen(
    "eams-submission-date",
    "medicines_eams_pim.will_submit_to_eams",
    YesNoUnknown.Yes
)]
[FormPageShowWhen("eams-opinion-date", "medicines_eams_pim.will_submit_to_eams", YesNoUnknown.Yes)]
[FormPageShowWhen(
    "eams-opinion-decision",
    "medicines_eams_pim.eams_opinion_date",
    DatePrecision.ActualDate
)]
internal sealed class MedicinesEamsPim
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.PimAndEams,
        MedicineFormTasks.PimAndEamsTitle,
        Order = MedicineFormTasks.PimAndEamsOrder
    )]
    [FormPage("pim-designation", "PIM designation", Order = 10, CommercialInConfidence = true)]
    [FormField(
        "medicines_eams_pim.pim_designation_status",
        "Do you have, or are you seeking, Medicines and Healthcare Regulatory Agency (MHRA) PIM designation?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select the current status of PIM designation for the indication in scope. Granted should only be selected once formally awarded by MHRA. If application is planned or under review, select the relevant pending option."
    )]
    [FormOptions(OptionKeys.PimDesignationStatus)]
    [FormRequired("Select granted if your product has been awarded MHRA PIM designation")]
    public DesignationStatus? PimDesignationStatus { get; set; }

    /// <summary>Conditional on PimDesignationStatus = Granted.</summary>
    [FormTask(
        MedicineFormTasks.PimAndEams,
        MedicineFormTasks.PimAndEamsTitle,
        Order = MedicineFormTasks.PimAndEamsOrder
    )]
    [FormPage("pim-designation", "PIM designation", Order = 10, CommercialInConfidence = true)]
    [FormField(
        "medicines_eams_pim.will_submit_to_eams",
        "Have you already or do you plan to submit to EAMS?",
        FormControl.Radio,
        Order = 20,
        Hint = "Select yes if you intend to pursue EAMS. Select no if you do not. Select unknown if undecided."
    )]
    [FormShowWhen("medicines_eams_pim.pim_designation_status", DesignationStatus.Granted)]
    [FormRequired("Select yes if you intend to submit or have submitted to EAMS")]
    public YesNoUnknown? WillSubmitToEams { get; set; }

    [FormTask(
        MedicineFormTasks.PimAndEams,
        MedicineFormTasks.PimAndEamsTitle,
        Order = MedicineFormTasks.PimAndEamsOrder
    )]
    [FormPage(
        "eams-submission-date",
        "EAMS submission date",
        Order = 20,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_eams_pim.eams_submission_date",
        "What is the EAMS submission date?",
        FormControl.PrecisionDate,
        Order = 10,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect to submit for EAMS for this product, or enter the actual date of submission."
    )]
    [FormDateEvent(DateEventType.EamsSubmission)]
    [FormRequired("Select an option")]
    public int? EamsSubmissionDateId { get; set; }

    [FormTask(
        MedicineFormTasks.PimAndEams,
        MedicineFormTasks.PimAndEamsTitle,
        Order = MedicineFormTasks.PimAndEamsOrder
    )]
    [FormPage(
        "eams-opinion-date",
        "EAMS scientific opinion date",
        Order = 30,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_eams_pim.eams_opinion_date",
        "Date of EAMS scientific opinion",
        FormControl.PrecisionDate,
        Order = 10,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect to receive the EAMS scientific opinion, or enter the actual date you received it."
    )]
    [FormDateEvent(DateEventType.EamsOpinion)]
    [FormRequired("Select an option")]
    public int? EamsOpinionDateId { get; set; }

    [FormTask(
        MedicineFormTasks.PimAndEams,
        MedicineFormTasks.PimAndEamsTitle,
        Order = MedicineFormTasks.PimAndEamsOrder
    )]
    [FormPage(
        "eams-opinion-decision",
        "EAMS scientific opinion decision",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_eams_pim.eams_opinion_decision",
        "What was the EAMS scientific opinion decision?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select a decision")]
    public EamsOpinionDecision? EamsOpinionDecision { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsOpinionDate { get; set; }
}
