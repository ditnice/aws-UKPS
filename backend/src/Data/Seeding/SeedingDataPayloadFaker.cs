using Bogus;
using UKPS.Api.Data.Fakers;
using UKPS.Api.Entities.Identity;

namespace UKPS.Api.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    public SeedingDataPayloadFaker()
    {
        var organisationFaker = new OrganisationFaker();
        var userFaker = new UserFaker();
        var membershipFaker = new UserOrgMembershipFaker();

        RuleFor(x => x.Organisations, (f, o) => organisationFaker.Generate(5));
        RuleFor(x => x.Users, (f, o) => userFaker.Generate(20));
        RuleFor(
            x => x.Memberships,
            (f, o) =>
            {
                return o
                    .Users.Select(u =>
                    {
                        Organisation selectedOrganisation = f.PickRandom<Organisation>(
                            o.Organisations
                        );
                        UserOrgMembership generatedMembership = membershipFaker.Generate();
                        generatedMembership.UserId = u.Id;
                        generatedMembership.OrganisationId = selectedOrganisation.Id;
                        return generatedMembership;
                    })
                    .ToArray();
            }
        );
    }
}
