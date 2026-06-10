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
