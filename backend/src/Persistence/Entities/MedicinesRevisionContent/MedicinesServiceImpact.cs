using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesServiceImpact
{
    public int Id { get; set; }
    public int RevisionId { get; set; }

    /// <summary>
    /// The extent of NHS service change required to deliver this medicine.
    /// </summary>
    public NhsServiceChangesRequired? NhsServiceChangesRequired { get; set; }

    /// <summary>
    /// Conditional on NhsServiceChangesRequired = CompleteTransformation or SomeChange.
    /// Describes changes to staffing, equipment, pathway, or commissioning.
    /// </summary>
    public string? NhsServiceChangesDetails { get; set; }

    /// <summary>
    /// Are there specific requirements for the handling or storage of this product
    /// that may cause service issues?
    /// </summary>
    public YesNoUnknown? HandlingStorageRequirements { get; set; }

    /// <summary>Conditional on HandlingStorageRequirements = Yes.</summary>
    public string? HandlingStorageDetails { get; set; }

    /// <summary>Estimated uptake based on expected adoption patterns. CiC.</summary>
    public string? EstimatedUptake { get; set; }

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
