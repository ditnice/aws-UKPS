using UKPS.Api.Entities.Common;
using UKPS.Api.Enums;

namespace UKPS.Api.Entities.Identity;

internal sealed class TermsAcceptance : EntityWithMetaData
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    public PharmaceuticalEntity RelevantPharmaceuticalEntity { get; set; }
    public required string SignatoryName { get; set; }
    public required string SignatoryEmail { get; set; }
    public string? SignatoryJobTitle { get; set; }
    public DateTime LinkExpiresAt { get; set; }
    public TermsAcceptanceStatus Status { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? IpAddress { get; set; }

    // Navigation
    public Organisation? Organisation { get; set; }
}
