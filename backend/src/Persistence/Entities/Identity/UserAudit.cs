using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserAudit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public IamEventType EventType { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? FieldPath { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User? User { get; set; }
    public User? UpdatedByUser { get; set; }
}
