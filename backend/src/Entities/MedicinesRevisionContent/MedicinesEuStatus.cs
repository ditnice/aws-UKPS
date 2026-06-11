using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

/// <summary>Owns the EuOrphanGranted regulatory date row.</summary>
internal sealed class MedicinesEuStatus
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public OrphanAtmpStatus? EuOrphanStatus { get; set; }
    public string? EuOrphanStatusNumber { get; set; }
    public int? EuOrphanGrantedDateId { get; set; }
    public OrphanAtmpStatus? EuAtmpClassificationStatus { get; set; }
    public int? AtmpClassificationId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public SharedRevisionContent.RegulatoryDate? EuOrphanGrantedDate { get; set; }
    public ReferenceData.AtmpClassification? AtmpClassification { get; set; }
}
