using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Fakers;

internal sealed class UserOrgMembershipFaker : Faker<UserOrgMembership>
{
    private static readonly UserOrgMembershipStatus[] _statuses =
        Enum.GetValues<UserOrgMembershipStatus>();

    public UserOrgMembershipFaker()
    {
        RuleFor(x => x.UserRole, f => f.PickRandom<UserRole>());
        RuleFor(x => x.Status, f => f.PickRandom(_statuses));
        RuleFor(x => x.AllowedPharmaceuticalEntity, f => f.PickRandom<PharmaceuticalEntity>());
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
    }
}
