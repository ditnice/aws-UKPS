using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Owns the EuOrphanGranted and AtmpClassificationRecommendation regulatory date rows.</summary>
internal sealed class MedicinesEuStatus
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public DesignationStatus? EuOrphanStatus { get; set; }

    /// <summary>Conditional on EuOrphanStatus = Granted.</summary>
    public string? EuOrphanStatusNumber { get; set; }

    /// <summary>Conditional on EuOrphanStatus = Granted.</summary>
    public int? EuOrphanGrantedDateId { get; set; }

    /// <summary>The ATMP question does not offer DesignationStatus.NoSubmissionIntended.</summary>
    public DesignationStatus? EuAtmpClassificationStatus { get; set; }

    /// <summary>
    /// Date the ATMP classification recommendation was provided.
    /// Conditional on EuAtmpClassificationStatus = Granted.
    /// </summary>
    public int? AtmpRecommendationDateId { get; set; }

    /// <summary>Conditional on EuAtmpClassificationStatus = Granted.</summary>
    public int? AtmpClassificationId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? EuOrphanGrantedDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? AtmpRecommendationDate { get; set; }
    public ReferenceData.AtmpClassification? AtmpClassification { get; set; }
}
