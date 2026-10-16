using System.Text.Json.Nodes;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Application.Forms.Rules;

/// <summary>
/// A validation rule on a single question. Every rule is serialised to the client
/// (<see cref="Kind"/>, <see cref="Value"/>, <see cref="Message"/>) and mirrored there, and
/// re-run on the server against the normalised answer.
/// </summary>
internal abstract record QuestionRule(string Message)
{
    public abstract RuleKind Kind { get; }

    /// <summary>The rule's parameter, if it has one (e.g. the maximum length).</summary>
    public virtual int? Value => null;

    /// <summary>Question types this rule may be attached to.</summary>
    public abstract IReadOnlySet<QuestionType> AppliesTo { get; }

    public abstract bool IsSatisfiedBy(JsonNode? value);
}
