namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>
/// A validation rule kind. Rules are evaluated in the order of these values, never in attribute
/// order. Part of the HTTP API contract.
/// </summary>
public enum RuleKind
{
    /// <summary>The question must be answered.</summary>
    Required = 0,

    /// <summary>Text must be at most a number of characters after trimming.</summary>
    MaxLength = 1,

    /// <summary>At most a number of options may be chosen.</summary>
    MaxItems = 2,
}
