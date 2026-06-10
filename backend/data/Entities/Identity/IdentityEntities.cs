using UKPS.Data.Enums;

namespace UKPS.Data.Entities.Identity;

public class Organisation
{
    public int Id { get; set; }
    public string OrganisationName { get; set; } = null!;
    public OrganisationType OrganisationType { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public string? CountryOrRegion { get; set; }
    public string? HeadOfficeAddress { get; set; }
    public string? HeadOfficeTelephone { get; set; }
    public string? HeadOfficeEmail { get; set; }
    public UserOrgStatus Status { get; set; }
    public DateTime? LastActive { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public ICollection<TermsAcceptance> TermsAcceptances { get; set; } = [];
    public ICollection<OrganisationAudit> OrganisationAudits { get; set; } = [];
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
}

public class TermsAcceptance
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

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public UserType UserType { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? WorkTelephone { get; set; }
    public string? WorkEmail { get; set; }

    // TODO: Cognito-managed placeholders — to be replaced when implementing Cognito
    public DateTime? LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public DateTime? LastActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserOrgMembership> UserOrgMemberships { get; set; } = [];
    public ICollection<UserAudit> UserAudits { get; set; } = [];
}

public class UserOrgMembership
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganisationId { get; set; }
    public UserRole UserRole { get; set; }
    public UserOrgStatus Status { get; set; }
    public PharmaceuticalEntity AllowedPharmaceuticalEntity { get; set; }
    public DateTime? CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Organisation Organisation { get; set; } = null!;
}

public class UserAudit
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
    public User User { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
}
