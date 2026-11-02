using UKPS.Api.Application.RecordForms.Model;

namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>A validation rule, with the message to show when it fails.</summary>
public sealed record RecordPageRuleDto
{
    /// <summary>The kind of rule.</summary>
    public required RuleKind Kind { get; init; }

    /// <summary>The rule's limit, for example the maximum length; null for Required.</summary>
    public required int? Value { get; init; }

    /// <summary>The message shown when the rule fails.</summary>
    public required string Message { get; init; }
}
