using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class TermsAcceptance
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public PharmaceuticalEntity RelevantPharmaceuticalEntity { get; set; }
    public string SignatoryName { get; set; } = null!;
    public string SignatoryEmail { get; set; } = null!;
    public string? SignatoryJobTitle { get; set; }
    public DateTime LinkExpiresAt { get; set; }
    public TermsAcceptanceStatus Status { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public Organisation Organisation { get; set; } = null!;
}
