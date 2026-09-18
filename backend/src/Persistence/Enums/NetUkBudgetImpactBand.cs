namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Estimated net UK budget impact over the first 3 years of NHS use.
/// An annual impact of £40m or more triggers specific NICE planning processes.
/// Bands are mutually exclusive and exhaustive:
///   LessThan5M      = strictly less than £5m
///   Between5MAnd40M = greater than or equal to £5m and strictly less than £40m
///   FortyMOrMore    = greater than or equal to £40m
///   Unknown         = not yet known
/// </summary>
public enum NetUkBudgetImpactBand
{
    /// <summary>The net UK budget impact is not yet known.</summary>
    Unknown = 0,

    /// <summary>Less than £5 million.</summary>
    LessThan5M = 1,

    /// <summary>£5 million to less than £40 million.</summary>
    Between5MAnd40M = 2,

    /// <summary>£40 million or more.</summary>
    FortyMOrMore = 3,
}
