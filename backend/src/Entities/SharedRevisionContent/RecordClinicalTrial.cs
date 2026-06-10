using UKPS.Data.Enums;

namespace UKPS.Data.Entities.SharedRevisionContent;

public class RecordClinicalTrial
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public string StudyName { get; set; } = null!;

    /// <summary>8-digit NCT identifier; system auto-links to ClinicalTrials.gov.</summary>
    public string? ClinicalTrialsGovNumber { get; set; }

    public string? BriefDescription { get; set; }
    public YesNoUnknown? RecruitingInUk { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ICollection<OtherClinicalTrialNumber> OtherClinicalTrialNumbers { get; set; } = [];
}
