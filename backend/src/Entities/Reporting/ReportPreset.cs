using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Reporting;

/// <summary>
/// Saved report configurations. Two tiers:
///   is_shared = true:  visible to all users of applicable_user_type. QA/IT admin only.
///   is_shared = false: personal preset, visible only to the creating user.
/// configuration jsonb stores the full report state — custom filter tree or
/// strategic report category selections.
/// </summary>
internal sealed class ReportPreset
{
    public int Id { get; set; }
    public UserType ApplicableUserType { get; set; }
    public PharmaceuticalEntity ApplicablePharmaceuticalEntity { get; set; }
    public string Title { get; set; } = null!;

    /// <summary>
    /// Full report config at time of save. Stored as jsonb.
    /// CustomReport: date field, date range, watchlist flag, rule/group filter tree.
    /// StrategicReport: date field, primary category field + selected options,
    /// optional secondary category field + selected options.
    /// </summary>
    public ReportPresetConfigurationDocument Configuration { get; set; } = new();

    public bool IsShared { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Identity.User CreatedByUser { get; set; } = null!;
    public ICollection<ReportAudit> ReportAudits { get; set; } = [];
}
