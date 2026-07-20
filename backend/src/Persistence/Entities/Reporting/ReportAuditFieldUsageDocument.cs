using System.Text.Json.Serialization;

namespace UKPS.Api.Persistence.Entities.Reporting;

/// <summary>Placeholder contents; replace with the final report audit field usage shape.</summary>
internal sealed class ReportAuditFieldUsageDocument
{
    [JsonPropertyName("filters")]
    public string[] Filters { get; set; } = [];

    [JsonPropertyName("exported")]
    public string[] Exported { get; set; } = [];
}
