using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    const int UsersPerOrganisation = 16;
    private readonly OrganisationFaker _organisationFaker = new OrganisationFaker();
    private readonly UserFaker _userFaker = new UserFaker();
    private readonly UserOrgMembershipFaker _membershipFaker = new UserOrgMembershipFaker();
    private readonly UserOrgStatus[] _statuses = Enum.GetValues<UserOrgStatus>();

    public SeedingDataPayloadFaker()
    {
        RuleFor(x => x.Organisations, (f, o) => _organisationFaker.Generate(5));
        RuleFor(
            x => x.Users,
            (f, o) =>
            {
                return o
                    .Organisations.SelectMany(_ => _userFaker.Generate(UsersPerOrganisation))
                    .ToArray();
            }
        );
        RuleFor(x => x.Memberships, (f, o) => FakeMemberships(o));
    }

    private UserOrgMembership[] FakeMemberships(SeedingDataPayload o)
    {
        // This membership is added to enable mock auth access.
        var mockUserMembership = _membershipFaker
            .RuleFor(x => x.Organisation, (f, _) => o.Organisations.First())
            .RuleFor(
                x => x.User,
                (f, _) =>
                    _userFaker.RuleFor(
                        x => x.WorkEmail,
                        _ => DevAuthenticationClaims.DefaultUserEmail
                    )
            )
            .RuleFor(x => x.UserRole, _ => UserRole.Super);
        var otherMemberships = o.Organisations.SelectMany(
            (org, orgIndex) =>
            {
                return o
                    .Users.Skip(orgIndex * UsersPerOrganisation)
                    .Take(UsersPerOrganisation)
                    .Select(
                        (u, i) =>
                        {
                            UserOrgMembership generatedMembership = _membershipFaker
                                .RuleFor(
                                    x => x.Status,
                                    _ =>
                                    {
                                        // Cycle through every status at least once per organisation for variety.
                                        return _statuses[i % _statuses.Length];
                                    }
                                )
                                .Generate();
                            generatedMembership.User = u;
                            generatedMembership.Organisation = org;
                            return generatedMembership;
                        }
                    );
            }
        );

        return otherMemberships.Append(mockUserMembership.Generate()).ToArray();
    }
}
