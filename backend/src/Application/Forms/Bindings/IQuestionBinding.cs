using System.Text.Json.Nodes;

namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Maps a question's answer onto its typed column or table. Bindings are the only form code
/// that touches EF entities. Values are wire JSON; see
/// <see cref="Definitions.AnswerValues"/>.
/// </summary>
internal interface IQuestionBinding
{
    BindingValueKind ValueKind { get; }

    /// <summary>The bound enum type when <see cref="ValueKind"/> is <see cref="BindingValueKind.Enum"/>.</summary>
    Type? EnumType { get; }

    /// <summary>A stable description used in the definition fingerprint.</summary>
    string Description { get; }

    Task<JsonNode?> ReadAsync(BindingContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Writes a normalised, validated answer. Changes are tracked; the caller saves them.
    /// </summary>
    Task WriteAsync(BindingContext context, JsonNode? value, CancellationToken cancellationToken);
}
