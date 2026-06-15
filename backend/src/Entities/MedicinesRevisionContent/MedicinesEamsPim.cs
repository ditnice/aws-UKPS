using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

/// <summary>Owns EamsSubmission and EamsOpinion regulatory date rows.</summary>
internal sealed class MedicinesEamsPim
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public PimDesignationStatus? PimDesignationStatus { get; set; }

    /// <summary>Conditional on PimDesignationStatus = Granted.</summary>
    public YesNoUnknown? WillSubmitToEams { get; set; }

    public EamsOpinionDecision? EamsOpinionDecision { get; set; }
    public int? EamsSubmissionDateId { get; set; }
    public int? EamsOpinionDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public SharedRevisionContent.RegulatoryDate? EamsSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? EamsOpinionDate { get; set; }
}
