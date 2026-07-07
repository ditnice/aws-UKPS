using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class OrganisationAudit
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public IamEventType EventType { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? FieldPath { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Organisation? Organisation { get; set; }
    public User? UpdatedByUser { get; set; }
}
