using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Health technology assessment and UK launch.</summary>
[FormSection(
    RecordType.Medicine,
    "regulatory-access-and-launch",
    "Provide regulatory, access, and launch information",
    Order = 30
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
        FormControl.PrefixedNumber,
        Order = 10,
        Hint = "Select the ID format and enter the NICE technology appraisal ID once it has been allocated by NICE. Leave blank if not yet known.",
        Placeholder = "Choose ID type"
    )]
    [FormValuePrefixes("GID-TA", "GID-HST")]
    public string? NiceTaDevelopmentId { get; set; }

    /// <summary>
    /// Medicines only. When the medicine is first available in the UK supply chain for this
    /// indication, stored as a regulatory date.
    /// </summary>
    [FormTask(
        MedicineFormTasks.HtaAndLaunch,
        MedicineFormTasks.HtaAndLaunchTitle,
        Order = MedicineFormTasks.HtaAndLaunchOrder
    )]
    [FormPage(
        "uk-launch-date",
        "UK commercial launch date",
        Order = 40,
        CommercialInConfidence = true
    )]
    [FormField(
        "record_hta.uk_launch_date",
        "What is the UK commercial launch date?",
        FormControl.PrecisionDate,
        Order = 10,
        Hint = "Choose the level of detail you are currently able to provide. Only enter an actual date if the medicine has already launched, meaning stock is already available to be administered or dispensed.\n\nThe commercial launch date is when this medicine will first be available in the UK supply chain for this indication. This is distinct from the licence date and from the date of any HTA recommendation or reimbursement decision. For new indications of a medicine already in the supply chain, this date will typically align with the licence date for that indication.\n\nFor medicines being made available through the Early Access to Medicines Scheme (EAMS), enter the date from which the medicine will be available under EAMS. This may be before the UK licence date."
    )]
    [FormDateEvent(DateEventType.UkLaunch, Name = "launch")]
    [FormRequired("Select an option")]
    public int? UkLaunchDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public RegulatoryDate? UkLaunchDate { get; set; }
}
