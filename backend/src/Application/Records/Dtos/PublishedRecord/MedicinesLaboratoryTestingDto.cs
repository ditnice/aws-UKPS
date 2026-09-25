using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the laboratory testing section of a medicine record.
/// </summary>
public sealed record MedicinesLaboratoryTestingDto
{
    /// <summary>
    /// Gets whether a diagnostic test is required.
    /// </summary>
    public YesNoUnknown? DiagnosticTestRequired { get; init; }

    /// <summary>
    /// Gets the biomarker type.
    /// </summary>
    public BiomarkerType? BiomarkerType { get; init; }

    /// <summary>
    /// Gets the description of a non-genomic biomarker.
    /// </summary>
    public string? NonGenomicBiomarkerDescription { get; init; }

    /// <summary>
    /// Gets the genomic target.
    /// </summary>
    public string? GenomicTarget { get; init; }

    /// <summary>
    /// Gets the relationship of the genomic test to the National Genomic Test Directory.
    /// </summary>
    public GenomicTestNgtdRelationship? GenomicTestNgtdRelationship { get; init; }

    /// <summary>
    /// Gets the genomic sample type.
    /// </summary>
    public string? GenomicSampleType { get; init; }

    /// <summary>
    /// Gets the genomic test turnaround time details.
    /// </summary>
    public string? GenomicTurnaroundTimeDetails { get; init; }

    /// <summary>
    /// Gets the point in the patient pathway at which the test is performed.
    /// </summary>
    public ReferenceDataDto? PatientPathwayPoint { get; init; }

    /// <summary>
    /// Gets the pathway point when not covered by the listed options.
    /// </summary>
    public string? GenomicTestPathwayPointOther { get; init; }

    /// <summary>
    /// Gets the genomic alterations.
    /// </summary>
    public string? GenomicAlterations { get; init; }

    /// <summary>
    /// Gets the genomic test used in trials.
    /// </summary>
    public string? GenomicTestUsedInTrials { get; init; }

    /// <summary>
    /// Gets the genomic test specificity and sensitivity.
    /// </summary>
    public string? GenomicTestSpecificitySensitivity { get; init; }

    /// <summary>
    /// Gets additional notes on the genomic test.
    /// </summary>
    public string? GenomicTestNotes { get; init; }

    /// <summary>
    /// Gets whether the genomic test is mandatory.
    /// </summary>
    public GenomicTestMandatoryStatus? GenomicTestMandatoryStatus { get; init; }

    /// <summary>
    /// Gets additional genomic factors.
    /// </summary>
    public string? AdditionalGenomicFactors { get; init; }

    /// <summary>
    /// Gets details of monitoring tests.
    /// </summary>
    public string? MonitoringTestsDetails { get; init; }

    /// <summary>
    /// Gets details of safety tests.
    /// </summary>
    public string? SafetyTestsDetails { get; init; }
}
