using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Identity;

public class OrganisationAudit
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
    public Organisation Organisation { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
}
