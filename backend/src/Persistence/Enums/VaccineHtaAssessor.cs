namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Organisation anticipated to assess the cost effectiveness of a vaccine.
/// Single-select.
/// </summary>
public enum VaccineHtaAssessor
{
    /// <summary>Joint Committee on Vaccination and Immunisation.</summary>
    Jcvi = 0,

    /// <summary>National Institute for Health and Care Excellence.</summary>
    Nice = 1,

    /// <summary>Neither body is expected to assess this vaccine.</summary>
    NotApplicable = 2,
}
