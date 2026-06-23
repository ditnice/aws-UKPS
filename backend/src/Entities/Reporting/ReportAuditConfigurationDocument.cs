using System.Text.Json.Serialization;

namespace UKPS.Api.Entities.Reporting;

/// <summary>Placeholder contents; replace with the final report audit configuration shape.</summary>
internal sealed class ReportAuditConfigurationDocument
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
