using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Answers about the record's clinical trials as a whole.</summary>
internal sealed class RecordClinicalTrialInformation
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Are any of the record's clinical trials recruiting in the UK?</summary>
    public YesNoUnknown? RecruitingInUk { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
