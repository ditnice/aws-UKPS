using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    public SeedingDataPayloadFaker()
    {
        const int usersPerOrganisation = 16;

        var organisationFaker = new OrganisationFaker();
        var userFaker = new UserFaker();
        var membershipFaker = new UserOrgMembershipFaker();

        RuleFor(x => x.Organisations, (f, o) => organisationFaker.Generate(5));
        RuleFor(
            x => x.Users,
            (f, o) =>
                o.Organisations.SelectMany(_ => userFaker.Generate(usersPerOrganisation)).ToArray()
        );
        RuleFor(
            x => x.Memberships,
            (f, o) =>
            {
                return o
                    .Organisations.SelectMany(
                        (org, orgIndex) =>
                        {
                            return o
                                .Users.Skip(orgIndex * usersPerOrganisation)
                                .Take(usersPerOrganisation)
                                .Select(
                                    (u, i) =>
                                    {
                                        UserOrgMembership generatedMembership =
                                            membershipFaker.Generate();
                                        generatedMembership.User = u;
                                        generatedMembership.Organisation = org;
                                        return generatedMembership;
                                    }
                                );
                        }
                    )
                    .ToArray();
            }
        );
    }
}
