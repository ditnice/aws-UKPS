namespace UKPS.Api.Application.Forms.Options;

/// <summary>A selectable option. Values are always strings on the wire.</summary>
internal sealed record QuestionOption(string Value, string Label);
