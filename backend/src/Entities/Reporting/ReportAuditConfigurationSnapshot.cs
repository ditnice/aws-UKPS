using System.Text.Json.Serialization;

namespace UKPS.Api.Entities.Reporting;

internal sealed class ReportAuditConfigurationSnapshot
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
