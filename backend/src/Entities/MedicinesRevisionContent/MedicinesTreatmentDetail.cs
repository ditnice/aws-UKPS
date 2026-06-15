using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

internal sealed class MedicinesTreatmentDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>Includes likely comparators.</summary>
    public string ProposedPlaceInTherapy { get; set; } = null!;

    /// <summary>CiC — Commercially in Confidence.</summary>
    public string? EstimatedDurationOfTreatment { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
}
