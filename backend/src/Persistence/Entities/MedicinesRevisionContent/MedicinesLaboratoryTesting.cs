using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

internal sealed class MedicinesLaboratoryTesting
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public YesNoUnknown? DiagnosticTestRequired { get; set; }

    /// <summary>
    /// Determines the biomarker type. Conditional on DiagnosticTestRequired = Yes.
    /// </summary>
    public BiomarkerType? BiomarkerType { get; set; }

    /// <summary>
    /// Conditional on BiomarkerType = NonGenomicBiomarker.
    /// Describes the non-genomic biomarker and how it is measured.
    /// </summary>
    public string? NonGenomicBiomarkerDescription { get; set; }

    /// <summary>
    /// The specific gene or genomic target the test detects.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    public string? GenomicTarget { get; set; }

    /// <summary>
    /// How the test relates to what is currently in the National Genomic Test Directory.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    public GenomicTestNgtdRelationship? GenomicTestNgtdRelationship { get; set; }

    /// <summary>Free text. Conditional on BiomarkerType = GenomicBiomarker.</summary>
    public string? GenomicSampleType { get; set; }

    /// <summary>
    /// Describes turnaround time requirements differing from the current commissioned pathway.
    /// Optional. Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    public string? GenomicTurnaroundTimeDetails { get; set; }

    public int? PatientPathwayPointId { get; set; }
    public string? GenomicTestPathwayPointOther { get; set; }

    /// <summary>What genomic alterations determine patient eligibility?</summary>
    public string? GenomicAlterations { get; set; }

    public string? GenomicTestUsedInTrials { get; set; }
    public string? GenomicTestSpecificitySensitivity { get; set; }
    public string? GenomicTestNotes { get; set; }

    /// <summary>Conditional on DiagnosticTestRequired = Yes.</summary>
    public GenomicTestMandatoryStatus? GenomicTestMandatoryStatus { get; set; }

    /// <summary>
    /// Additional genomic factors that affect treatment selection or sequencing
    /// beyond the primary eligibility alteration.
    /// Conditional on BiomarkerType = GenomicBiomarker.
    /// </summary>
    public string? AdditionalGenomicFactors { get; set; }

    /// <summary>
    /// Tests needed to monitor response to treatment beyond current NHS practice.
    /// Optional. Not conditional — shown regardless of biomarker type. CiC.
    /// </summary>
    public string? MonitoringTestsDetails { get; set; }

    /// <summary>
    /// Tests needed to assess safety of treatment beyond current NHS practice.
    /// Optional. Not conditional — shown regardless of biomarker type. CiC.
    /// </summary>
    public string? SafetyTestsDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.PatientPathwayPoint? PatientPathwayPoint { get; set; }
}
