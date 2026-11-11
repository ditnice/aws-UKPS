using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Medicines only — the global first submission of this product.</summary>
[FormSection(
    RecordType.Medicine,
    "regulatory-access-and-launch",
    "Provide regulatory, access, and launch information",
    Order = 30
)]
[FormPageShowWhen(
    "global-first-submission",
    "record_mhra_date.uk_submission_date",
    DatePrecision.ActualDate
)]
internal sealed class MedicinesGlobalSubmission
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>
    /// Conditional on an actual UK submission date being entered.
    /// The region or regulator responsible for the global first submission.
    /// </summary>
    [FormTask(
        MedicineFormTasks.MhraProcedureAndDates,
        MedicineFormTasks.MhraProcedureAndDatesTitle,
        Order = MedicineFormTasks.MhraProcedureAndDatesOrder
    )]
    [FormPage(
        "global-first-submission",
        "Global first submission (GFS)",
        Order = 30,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_global_submission.global_first_submission_region",
        "Enter the GFS location of this product",
        FormControl.Text,
        Order = 10,
        Hint = "Specify the geographic region or regulatory authority where the global first submission occurred."
    )]
    [FormRequired("Enter a location")]
    [FormMaxLength(200, "Global first submission must be 200 characters or fewer")]
    public string? GlobalFirstSubmissionRegion { get; set; }

    /// <summary>Conditional on an actual UK submission date being entered.</summary>
    [FormTask(
        MedicineFormTasks.MhraProcedureAndDates,
        MedicineFormTasks.MhraProcedureAndDatesTitle,
        Order = MedicineFormTasks.MhraProcedureAndDatesOrder
    )]
    [FormPage(
        "global-first-submission",
        "Global first submission (GFS)",
        Order = 30,
        CommercialInConfidence = true
    )]
    [FormField(
        "medicines_global_submission.global_submission_actual_date",
        "Actual global first submission date",
        FormControl.ActualMonth,
        Order = 20,
        Hint = "For example, 3 2027"
    )]
    [FormDateEvent(DateEventType.GlobalFirstSubmission)]
    [FormRequired("Enter the date of GFS")]
    public int? GlobalSubmissionActualDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? GlobalSubmissionActualDate { get; set; }
}
