using Bogus;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;

namespace UKPS.Api.Data.Fakers;

internal sealed class UserOrgMembershipFaker : Faker<UserOrgMembership>
{
    public UserOrgMembershipFaker()
    {
        RuleFor(x => x.Id, f => f.IndexFaker + 1);
        RuleFor(x => x.UserId, f => f.Random.Int(1, 1000));
        RuleFor(x => x.OrganisationId, f => f.Random.Int(1, 1000));
        RuleFor(x => x.UserRole, f => f.PickRandom<UserRole>());
        RuleFor(x => x.Status, f => f.PickRandom<UserOrgStatus>());
        RuleFor(x => x.AllowedPharmaceuticalEntity, f => f.PickRandom<PharmaceuticalEntity>());
        RuleFor(x => x.CreatedAt, f => f.Date.Past(5).ToUniversalTime());
    }
}
