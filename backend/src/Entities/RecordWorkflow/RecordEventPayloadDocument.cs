using System.Text.Json.Serialization;

namespace UKPS.Api.Entities.RecordWorkflow;

/// <summary>Placeholder contents; replace with the final record event payload shape.</summary>
internal sealed class RecordEventPayloadDocument
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
