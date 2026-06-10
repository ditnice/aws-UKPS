using System.Text.Json;
using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Reporting;

/// <summary>
/// Saved report configurations. Two tiers:
///   is_shared = true:  visible to all users of applicable_user_type. QA/IT admin only.
///   is_shared = false: personal preset, visible only to the creating user.
/// configuration jsonb stores the full report state — custom filter tree or
/// strategic report category selections.
/// </summary>
public class ReportPreset
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
    public JsonDocument Configuration { get; set; } = null!;

    public bool IsShared { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Identity.User CreatedByUser { get; set; } = null!;
    public ICollection<ReportAudit> ReportAudits { get; set; } = [];
}

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

namespace UKPS.Data.Entities.Email;

public class EmailTemplate
{
    public int Id { get; set; }

    /// <summary>e.g. RecordApproved, RecordRejected, UserRegistrationApproved, TermsAcceptanceRequest</summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>External template ID from GovNotify if used.</summary>
    public string? GovnotifyTemplateId { get; set; }

    /// <summary>May be managed externally by GovNotify; retained for reference.</summary>
    public string? Subject { get; set; }

    /// <summary>May be managed externally by GovNotify; retained for reference.</summary>
    public string? Body { get; set; }

    public bool IsActive { get; set; }

    // Navigation
    public ICollection<EmailAudit> EmailAudits { get; set; } = [];
}

public class EmailAudit
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public DateTime SentAt { get; set; }
    public string Recipients { get; set; } = null!;

    /// <summary>
    /// Soft reference — e.g. record, user, organisation.
    /// No FK constraint: a single table cannot FK to multiple entity tables simultaneously.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Soft reference: PK of the related record.</summary>
    public int? RelatedEntityId { get; set; }

    // Navigation
    public EmailTemplate Template { get; set; } = null!;
}
