namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Determines whether the required biomarker is genomic or non-genomic.
/// </summary>
public enum BiomarkerType
{
    /// <summary>Unknown at this stage.</summary>
    Unknown = 0,

    /// <summary>A non-genomic biomarker determines eligibility.</summary>
    NonGenomicBiomarker = 1,

    /// <summary>A genomic biomarker determines eligibility.</summary>
    GenomicBiomarker = 2,
}
