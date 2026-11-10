using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Health technology assessment and UK launch.</summary>
[FormSection(
    RecordType.Medicine,
    "hta-and-launch",
    "Health technology assessment and launch",
    Order = 50
)]
[FormPageShowWhen(
    "nice-aligned-pathway",
    "record_hta.medicine_hta_bodies",
    MedicineHtaAssessor.Nice
)]
[FormPageShowWhen("nice-ta-id", "record_hta.medicine_hta_bodies", MedicineHtaAssessor.Nice)]
internal sealed class RecordHta
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>Vaccines only: JCVI, NICE, or Not applicable (single-select radio).</summary>
    [FormIgnore("Vaccine form only")]
    public VaccineHtaAssessor? VaccineHtaAssessor { get; set; }

    /// <summary>
    /// Medicines only: is an HTA submission planned for this indication?
    /// </summary>
    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage("hta-submission", "HTA submission", Order = 10)]
    [FormField(
        "record_hta.medicine_hta_submission_intended",
        "Do you intend to submit for a HTA?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select yes if you intend to submit for a HTA")]
    public YesNoUnknown? MedicineHtaSubmissionIntended { get; set; }

    /// <summary>
    /// Medicines only, multi-select.
    /// </summary>
    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage("hta-submission", "HTA submission", Order = 10)]
    [FormField(
        "record_hta.medicine_hta_bodies",
        "Select the submission bodies you intend to submit to for a HTA",
        FormControl.Checkbox,
        Order = 20,
        Hint = "Access information on the different routes of assessment for HTA bodies."
    )]
    [FormShowWhen("record_hta.medicine_hta_submission_intended", YesNoUnknown.Yes)]
    [FormRequired("Select if you will submit to NICE, the SMC, or AWMSG for a HTA")]
    public MedicineHtaAssessor? MedicineHtaBodies { get; set; }

    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage("hta-submission", "HTA submission", Order = 10)]
    [FormField(
        "record_hta.hta_additional_details",
        "Add additional details or clarifications, such as any planned differences in the indications submitted to each HTA",
        FormControl.Textarea,
        Order = 30
    )]
    [FormShowWhen("record_hta.medicine_hta_submission_intended", YesNoUnknown.Yes)]
    [FormRequired("Enter details")]
    public string? HtaAdditionalDetails { get; set; }

    /// <summary>Medicines only. Conditional on NICE being selected.</summary>
    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage("nice-aligned-pathway", "MHRA/NICE aligned pathway", Order = 20)]
    [FormField(
        "record_hta.hta_nice_aligned_pathway",
        "Do you intend to follow the MHRA/NICE aligned pathway?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select yes if you intend to follow the MHRA/NICE aligned pathway")]
    public YesNoUnknown? HtaNiceAlignedPathway { get; set; }

    /// <summary>
    /// Medicines only. The unique identifier assigned by NICE to this technology appraisal.
    /// Format: GID-TAXXXX or GID-HSTXXXX. Optional.
    /// </summary>
    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage("nice-ta-id", "NICE technology appraisal ID", Order = 30)]
    [FormField(
        "record_hta.nice_ta_development_id",
        "Enter the NICE technology appraisal development ID for this product (optional)",
        FormControl.Text,
        Order = 10,
        Hint = "Enter the NICE technology appraisal ID once it has been allocated by NICE, starting GID-TA or GID-HST followed by the number. Leave blank if not yet known."
    )]
    public string? NiceTaDevelopmentId { get; set; }

    [FormIgnore("Precision dates not supported yet")]
    public int? UkLaunchDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public RegulatoryDate? UkLaunchDate { get; set; }
}
