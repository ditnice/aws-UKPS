namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Used where only Yes/No is appropriate — there is no Unknown option.
/// Kept separate from <see cref="YesNoUnknown"/> for type safety.
/// </summary>
public enum YesNo
{
    /// <summary>
    /// Indicates a negative response or denial.
    /// </summary>
    No = 0,

    /// <summary>
    /// Indicates a positive response or affirmation.
    /// </summary>
    Yes = 1,
}
