using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class UserOrgMembershipFaker : Faker<UserOrgMembership>
{
    // Rejected memberships are hidden by user queries, so generating them by default makes
    // any test that relies on row counts flaky. Assign it explicitly where it is needed.
    private static readonly UserOrgStatus[] _statuses =
    [
        .. Enum.GetValues<UserOrgStatus>().Where(s => s != UserOrgStatus.Rejected),
    ];

    public UserOrgMembershipFaker()
    {
        RuleFor(x => x.UserRole, f => f.PickRandom<UserRole>());
        RuleFor(x => x.Status, f => f.PickRandom(_statuses));
        RuleFor(x => x.AllowedPharmaceuticalEntity, f => f.PickRandom<PharmaceuticalEntity>());
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
    }
}
