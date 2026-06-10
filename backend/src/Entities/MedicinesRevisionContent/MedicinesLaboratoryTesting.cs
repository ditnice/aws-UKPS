using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

public class MedicinesLaboratoryTesting
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? DiagnosticTestRequired { get; set; }
    public YesNoUnknown? GenomicTestRequired { get; set; }
    public YesNoUnknown? GenomicTestInNationalDirectory { get; set; }
    public string? NationalGenomicTestDirectoryId { get; set; }
    public int? GenomicSampleTypeId { get; set; }
    public string? GenomicSampleTypeOther { get; set; }
    public YesNoUnknown? GenomicTurnaroundConsiderations { get; set; }
    public int? PatientPathwayPointId { get; set; }
    public string? GenomicTestPathwayPointOther { get; set; }
    public string? GenomicBiomarker { get; set; }
    public string? GenomicAlterations { get; set; }
    public string? GenomicTestUsedInTrials { get; set; }
    public string? GenomicTestSpecificitySensitivity { get; set; }
    public string? GenomicCoMutations { get; set; }
    public YesNoUnknown? GenomicTestMandatory { get; set; }
    public string? GenomicTestNotes { get; set; }
    public YesNoUnknown? MonitoringTestsRequired { get; set; }

    /// <summary>Conditional on MonitoringTestsRequired = Yes.</summary>
    public string? MonitoringTestsDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.GenomicSampleType? GenomicSampleType { get; set; }
    public ReferenceData.PatientPathwayPoint? PatientPathwayPoint { get; set; }
}
