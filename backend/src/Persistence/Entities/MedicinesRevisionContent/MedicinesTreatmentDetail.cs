namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesTreatmentDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Includes likely comparators.</summary>
    public required string ProposedPlaceInTherapy { get; set; }

    /// <summary>CiC — Commercially in Confidence.</summary>
    public string? EstimatedDurationOfTreatment { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
