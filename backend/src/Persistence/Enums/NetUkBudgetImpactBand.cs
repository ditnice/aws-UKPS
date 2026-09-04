namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Estimated net budget impact for the UK over the first 3 years of NHS use.
/// </summary>
public enum NetUkBudgetImpactBand
{
    /// <summary>The net UK budget impact is not yet known.</summary>
    Unknown = 0,

    /// <summary>Less than £5 million.</summary>
    LessThan5M = 1,

    /// <summary>Between £5 million and £40 million.</summary>
    Between5MAnd40M = 2,

    /// <summary>Over £40 million — triggers specific NICE planning processes.</summary>
    Over40M = 3,
}
