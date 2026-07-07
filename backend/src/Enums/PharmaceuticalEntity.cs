namespace UKPS.Api.Enums;

/// <summary>
/// Flags enum stored as integer. Only Organisation uses Both;
/// UserOrgMembership and TermsAcceptance use Medicines or Vaccines only.
/// </summary>
[Flags]
public enum PharmaceuticalEntity
{
    None = 0,
    Medicines = 1,
    Vaccines = 2,
    Both = Medicines | Vaccines,
}
