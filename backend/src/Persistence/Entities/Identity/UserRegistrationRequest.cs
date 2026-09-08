namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserRegistrationRequest
{
    public int Id { get; set; }
    public required int OrganisationId { get; set; }
    public required string FullName { get; set; }
    public required string WorkEmail { get; set; }
    public required string PhoneNumber { get; set; }
    public int? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    // Navigation
    public Organisation? Organisation { get; set; }
    public User? RejectedByUser { get; set; }
}
