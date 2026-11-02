namespace UKPS.Api.Application.RecordForms.Dtos;

/// <summary>An option of a choice question. Only hierarchical options have children.</summary>
public sealed record RecordPageOptionDto
{
    /// <summary>The answer value: an enum member name or a reference ID.</summary>
    public required string Value { get; init; }

    /// <summary>The text shown for the option.</summary>
    public required string Label { get; init; }

    /// <summary>The child options of a hierarchical option, or null.</summary>
    public required IReadOnlyList<RecordPageOptionDto>? Children { get; init; }
}
