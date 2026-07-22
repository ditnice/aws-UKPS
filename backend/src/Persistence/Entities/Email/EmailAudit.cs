namespace UKPS.Api.Persistence.Entities.Email;

internal sealed class EmailAudit
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public DateTime SentAt { get; set; }
    public required string Recipients { get; set; }

    /// <summary>
    /// Soft reference — e.g. record, user, organisation.
    /// No FK constraint: a single table cannot FK to multiple entity tables simultaneously.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Soft reference: PK of the related record.</summary>
    public int? RelatedEntityId { get; set; }

    // Navigation
    public EmailTemplate? Template { get; set; }
}
