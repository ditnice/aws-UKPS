using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>
/// Answers about the record's clinical trials as a whole, and the trials themselves, which share
/// its <see cref="RevisionId"/>.
/// </summary>
[FormEntity]
[FormPageShowWhen(
    MedicineFormLayout.ClinicalTrialDetails,
    "record_clinical_trial_information.has_clinical_trials",
    YesNo.Yes
)]
[FormPageShowWhen(
    MedicineFormLayout.RecruitingInUk,
    "record_clinical_trial_information.has_clinical_trials",
    YesNo.Yes
)]
internal sealed class RecordClinicalTrialInformation
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>Does the user wish to add clinical trials to the record?</summary>
    [FormPage(MedicineFormLayout.AddClinicalTrial)]
    [FormField(
        "record_clinical_trial_information.has_clinical_trials",
        "Do you wish to add a clinical trial?",
        FormControl.Radio,
        Hint = "Add multiple clinical trials that are relevant to the product seeking market authorisation."
    )]
    [FormRequired("Select an option")]
    public YesNo? HasClinicalTrials { get; set; }

    /// <summary>Are any of the record's clinical trials recruiting in the UK?</summary>
    [FormPage(MedicineFormLayout.RecruitingInUk)]
    [FormField(
        "record_clinical_trial_information.recruiting_in_uk",
        "Are any clinical trials recruiting in the UK?",
        FormControl.Radio
    )]
    [FormOptions(OptionKeys.ClinicalTrialsRecruitingInUk)]
    [FormRequired("Select an option")]
    public YesNoUnknown? RecruitingInUk { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }

    /// <summary>The revision's clinical trials, joined on <see cref="RevisionId"/>.</summary>
    [FormPage(MedicineFormLayout.ClinicalTrialDetails)]
    [FormField(
        "record_clinical_trial",
        "Enter clinical trial details",
        FormControl.Group,
        Hint = "Include all studies that support marketing authorisation and HTA submission.",
        ItemLabel = "Clinical trial",
        AddAnotherLabel = "Add another clinical trial",
        RemoveLabel = "Remove clinical trial"
    )]
    [FormRequired("Add at least one clinical trial")]
    public ICollection<RecordClinicalTrial> ClinicalTrials { get; set; } = [];
}
