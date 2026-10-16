namespace UKPS.Api.Application.Forms.Dtos;

/// <summary>A selectable option on a form question.</summary>
public sealed record FormOptionDto
{
    /// <summary>The value submitted when the option is chosen.</summary>
    public required string Value { get; init; }

    /// <summary>The option's display text.</summary>
    public required string Label { get; init; }
}
