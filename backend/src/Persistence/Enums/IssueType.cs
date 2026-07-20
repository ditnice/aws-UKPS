namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Represents the types of issues that can be encountered.
/// </summary>
public enum IssueType
{
    /// <summary>
    /// Indicates that a required detail is missing.
    /// </summary>
    MissingDetail = 0,

    /// <summary>
    /// Indicates that a value provided is invalid.
    /// </summary>
    InvalidValue = 1,

    /// <summary>
    /// Indicates that there is an inconsistency in the provided answers.
    /// </summary>
    InconsistentAnswer = 2,

    /// <summary>
    /// Represents any other type of issue not covered by the predefined types.
    /// </summary>
    Other = 3,
}
