namespace UKPS.Api.Application.Forms.Rules;

/// <summary>
/// The kind of a form validation rule. Clients mirror each kind; the server re-runs them all.
/// </summary>
public enum RuleKind
{
    /// <summary>The question must be answered (non-blank text, a selection, or at least one item).</summary>
    Required = 0,

    /// <summary>Text may be at most <c>value</c> characters (UTF-16 code units).</summary>
    MaxLength = 1,

    /// <summary>At most <c>value</c> options may be selected.</summary>
    MaxItems = 2,
}
