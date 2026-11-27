using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Medicines only — the global first submission of this product.</summary>
[FormEntity]
[FormPageShowWhen(
    MedicineFormLayout.GlobalFirstSubmission,
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
    [FormPage(MedicineFormLayout.GlobalFirstSubmission)]
    [FormField(
        "medicines_global_submission.global_first_submission_region",
        "Enter the location of the GFS of this product",
        FormControl.Text,
        Order = 10,
        Hint = "Specify the geographic region or regulatory authority where the global first submission occurred."
    )]
    [FormRequired("Enter a location")]
    public string? GlobalFirstSubmissionRegion { get; set; }

    /// <summary>Conditional on an actual UK submission date being entered.</summary>
    [FormPage(MedicineFormLayout.GlobalFirstSubmission)]
    [FormField(
        "medicines_global_submission.global_submission_actual_date",
        "Enter the date of GFS of this product",
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
