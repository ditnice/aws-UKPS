using UKPS.Data.Enums;

namespace UKPS.Data.Entities.MedicinesRevisionContent;

public class MedicinesTreatmentDetail
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
