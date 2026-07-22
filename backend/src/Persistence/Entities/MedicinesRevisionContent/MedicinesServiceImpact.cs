using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesServiceImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? ExistingNhsService { get; set; }
    public string? NhsServiceRedesignDetails { get; set; }
    public int? UkPatientPopulationRangeId { get; set; }
    public string? UkPatientPopulationNotes { get; set; }
    public string? EstimatedEligiblePatientPopulation { get; set; }
    public YesNoUnknown? CompassionateAccessAvailable { get; set; }

    /// <summary>Conditional on CompassionateAccessAvailable = Yes.</summary>
    public string? CompassionateAccessDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.UkPatientPopulationRange? UkPatientPopulationRange { get; set; }
}
