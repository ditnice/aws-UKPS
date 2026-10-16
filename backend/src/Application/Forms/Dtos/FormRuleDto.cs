using UKPS.Api.Application.Forms.Rules;

namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>A validation rule on a form question.</summary>
public sealed record FormRuleDto
{
    /// <summary>The kind of rule.</summary>
    public required RuleKind Kind { get; init; }

    /// <summary>The rule's parameter, if it has one (e.g. the maximum length).</summary>
    public int? Value { get; init; }

    /// <summary>The error message to show when the rule fails.</summary>
    public required string Message { get; init; }
}
