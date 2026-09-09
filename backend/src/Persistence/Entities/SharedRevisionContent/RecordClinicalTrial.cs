using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordClinicalTrial
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public required string StudyName { get; set; }

    /// <summary>8-digit NCT identifier; system auto-links to ClinicalTrials.gov. Mandatory for both record types.</summary>
    public required string ClinicalTrialsGovNumber { get; set; }

    public string? BriefDescription { get; set; }
    public YesNoUnknown? RecruitingInUk { get; set; }

    /// <summary>
    /// Vaccines only, where it is mandatory. Null for medicine records.
    /// Preclinical / Phase I / Phase I/II / Phase II / Phase III / Phase III/IV / Phase IV.
    /// </summary>
    public TrialPhase? TrialPhase { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ICollection<OtherClinicalTrialNumber> OtherClinicalTrialNumbers { get; set; } = [];
}
