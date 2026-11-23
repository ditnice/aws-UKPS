namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>An option chosen in an answer.</summary>
public sealed record RecordAnswersOptionDto
{
    /// <summary>The answer value: an enum member name or a reference ID.</summary>
    public required string Value { get; init; }

    /// <summary>The text shown for the option.</summary>
    public required string Label { get; init; }

    /// <summary>The label of the option's parent in a hierarchical select, or null.</summary>
    public required string? ParentLabel { get; init; }
}
