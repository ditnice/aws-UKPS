using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

[FormSection(
    RecordType.Medicine,
    "regulatory-access-and-launch",
    "Provide regulatory, access, and launch information",
    Order = 30
)]
internal sealed class RecordMhraDate
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.MhraProcedureAndDates,
        MedicineFormTasks.MhraProcedureAndDatesTitle,
        Order = MedicineFormTasks.MhraProcedureAndDatesOrder
    )]
    [FormPage(
        "uk-submission-date",
        "UK regulatory submission date",
        Order = 20,
        CommercialInConfidence = true
    )]
    [FormField(
        "record_mhra_date.uk_submission_date",
        "What is the UK regulatory submission date?",
        FormControl.PrecisionDate,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you plan to submit this product to the MHRA, or enter the actual date the dossier was formally accepted for review by the MHRA."
    )]
    [FormDateEvent(DateEventType.UkSubmission, Name = "submission")]
    [FormRequired("Select an option")]
    public int? UkSubmissionDateId { get; set; }

    [FormTask(
        MedicineFormTasks.MhraProcedureAndDates,
        MedicineFormTasks.MhraProcedureAndDatesTitle,
        Order = MedicineFormTasks.MhraProcedureAndDatesOrder
    )]
    [FormPage(
        "uk-licence-date",
        "UK regulatory licence date",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "record_mhra_date.uk_licence_date",
        "What is the UK regulatory licence date?",
        FormControl.PrecisionDate,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect MHRA will grant marketing authorisation for this indication, or enter the actual date MHRA granted marketing authorisation for the specific indication in scope. This refers to the regulatory approval date, not NICE recommendation or commercial launch."
    )]
    [FormDateEvent(DateEventType.UkLicence, Name = "licence")]
    [FormRequired("Select an option")]
    public int? UkLicenceDateId { get; set; }

    [FormTask(
        MedicineFormTasks.MhraProcedureAndDates,
        MedicineFormTasks.MhraProcedureAndDatesTitle,
        Order = MedicineFormTasks.MhraProcedureAndDatesOrder
    )]
    [FormPage(
        "uk-conditional-approval",
        "UK conditional approval",
        Order = 50,
        CommercialInConfidence = true
    )]
    [FormField(
        "record_mhra_date.uk_conditional_approval_anticipated",
        "Is conditional approval anticipated in the UK?",
        FormControl.Radio,
        Hint = "Select yes if you anticipate conditional approval based on current plans."
    )]
    [FormRequired("Select whether conditional approval is anticipated in the UK")]
    public YesNoUnknown? UkConditionalApprovalAnticipated { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public RegulatoryDate? UkSubmissionDate { get; set; }
    public RegulatoryDate? UkLicenceDate { get; set; }
}
