using System.Text.Json;
using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Reporting;

/// <summary>
/// Log of every report run for KPI tracking.
/// report_preset_id null = ad-hoc run without saving a preset.
/// configuration always stored at run time even when a preset was used,
/// since presets can be edited over time.
/// </summary>
public class ReportAudit
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Null if ad-hoc run without a saved preset.</summary>
    public int? ReportPresetId { get; set; }

    /// <summary>Full config at time of run. Always stored regardless of preset.</summary>
    public JsonDocument? Configuration { get; set; }

    /// <summary>
    /// Array of field names referenced in the configuration.
    /// Populated by the application layer. Enables the field usage in reports KPI.
    /// e.g. ["formulation_type", "uk_submission_date"]
    /// </summary>
    public JsonDocument? FieldUsage { get; set; }

    /// <summary>Number of records returned. Indicates whether the configuration was useful.</summary>
    public int? ResultCount { get; set; }

    /// <summary>True is a stronger signal of utility than a view alone.</summary>
    public bool? Exported { get; set; }

    public DateTime RanAt { get; set; }

    // Navigation
    public Identity.User User { get; set; } = null!;
    public ReportPreset? ReportPreset { get; set; }
}
