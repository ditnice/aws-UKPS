namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Clinical trial phase. Vaccines only.
/// </summary>
public enum TrialPhase
{
    /// <summary>Preclinical.</summary>
    Preclinical = 0,

    /// <summary>Phase I.</summary>
    PhaseI = 1,

    /// <summary>Phase I/II.</summary>
    PhaseIAndII = 2,

    /// <summary>Phase II.</summary>
    PhaseII = 3,

    /// <summary>Phase III.</summary>
    PhaseIII = 4,

    /// <summary>Phase III/IV.</summary>
    PhaseIIIAndIV = 5,

    /// <summary>Phase IV.</summary>
    PhaseIV = 6,
}
