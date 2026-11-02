namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A validation rule on a question, with the message shown when it fails.</summary>
internal sealed record FormRule(RuleKind Kind, int? Value, string Message);
