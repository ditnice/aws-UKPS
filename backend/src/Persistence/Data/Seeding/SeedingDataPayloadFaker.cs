using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    public SeedingDataPayloadFaker()
    {
        const int usersPerOrganisation = 16;

        var organisationFaker = new OrganisationFaker();
        var userFaker = new UserFaker();
        var membershipFaker = new UserOrgMembershipFaker();
        var statuses = Enum.GetValues<UserOrgStatus>();

        RuleFor(x => x.Organisations, (f, o) => organisationFaker.Generate(5));
        RuleFor(
            x => x.Users,
            (f, o) =>
            {
                return o
                    .Organisations.SelectMany(_ => userFaker.Generate(usersPerOrganisation))
                    .ToArray();
            }
        );
        RuleFor(
            x => x.Memberships,
            (f, o) =>
            {
                // This membership is added to enable mock auth access.
                var mockUserMembership = membershipFaker
                    .RuleFor(x => x.Organisation, (f, _) => o.Organisations.First())
                    .RuleFor(
                        x => x.User,
                        (f, _) =>
                            userFaker.RuleFor(
                                x => x.WorkEmail,
                                _ => DevAuthenticationOptions.DefaultUserEmail
                            )
                    )
                    .RuleFor(x => x.UserRole, _ => UserRole.Super);
                var otherMemberships = o
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
                                        // Cycle through every status at least once per organisation for variety.
                                        generatedMembership.Status = statuses[i % statuses.Length];
                                        return generatedMembership;
                                    }
                                );
                        }
                    )
                    .ToArray();
                return otherMemberships.Append(mockUserMembership.Generate()).ToArray();
            }
        );
    }
}
