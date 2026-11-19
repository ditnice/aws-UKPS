using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>
/// One of a revision's clinical trials. Its questions are asked of each item of the
/// <c>record_clinical_trial</c> group on <see cref="RecordClinicalTrialInformation"/>, so they
/// take that group's page.
/// </summary>
internal sealed class RecordClinicalTrial
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormIgnore("Display order is supplied by the server")]
    public int? DisplayOrder { get; set; }

    [FormField(
        "study_name",
        "Study name",
        FormControl.Text,
        Hint = "Enter the official or commonly used study title. Use consistent naming across UK PharmaScan and public trial registries.",
        Order = 10
    )]
    [FormRequired("Enter the study name")]
    public required string StudyName { get; set; }

    /// <summary>8-digit NCT identifier; system auto-links to ClinicalTrials.gov. Mandatory for both record types.</summary>
    [FormField(
        "clinical_trials_gov_number",
        "Trial number from clinical trials.gov",
        FormControl.PrefixedNumber,
        Hint = "Enter the 8 digit identification number that is assigned to any clinical trial registered on ClinicalTrials.gov.",
        Order = 20
    )]
    [FormValuePrefixes("NCT", Digits = 8)]
    [FormRequired("Enter the trial number from ClinicalTrials.gov")]
    [FormDistinct("Trial number from ClinicalTrials.gov must be different for each clinical trial")]
    public required string ClinicalTrialsGovNumber { get; set; }

    /// <summary>
    /// Vaccines only, where it is mandatory. Null for medicine records.
    /// Preclinical / Phase I / Phase I/II / Phase II / Phase III / Phase III/IV / Phase IV.
    /// </summary>
    [FormIgnore("Vaccines only")]
    public TrialPhase? TrialPhase { get; set; }

    /// <summary>Brief description of key findings. Vaccines only. Null for medicine records.</summary>
    [FormIgnore("Vaccines only")]
    public string? BriefDescription { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }

    [FormField(
        "other_registry_numbers",
        "Trial number from other registry (optional)",
        FormControl.TextList,
        Hint = "Enter any additional trial numbers from other registries.",
        AddAnotherLabel = "Add another trial number",
        ItemLabel = "Other registry number",
        RemoveLabel = "Remove",
        Order = 30
    )]
    [FormDistinct("Trial number from other registry must be different for each entry")]
    public ICollection<OtherClinicalTrialNumber> OtherClinicalTrialNumbers { get; set; } = [];
}
