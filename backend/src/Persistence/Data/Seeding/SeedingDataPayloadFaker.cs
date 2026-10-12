using System.Globalization;
using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Data.Seeding.SyntheticData;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    internal const int UsersPerOrganisation = 16;

    // The internal organisation is seeded first so that it gets ID 1, which the default
    // development user's claims refer to.
    private static readonly OrganisationType[] _organisationOrder =
    [
        OrganisationType.Internal,
        OrganisationType.PharmaCompany,
        OrganisationType.HorizonScanning,
        OrganisationType.Strategic,
    ];

    private static readonly Dictionary<OrganisationType, UserType> _userTypes = new Dictionary<
        OrganisationType,
        UserType
    >
    {
        [OrganisationType.Internal] = UserType.QaUser,
        [OrganisationType.PharmaCompany] = UserType.PharmaUser,
        [OrganisationType.HorizonScanning] = UserType.HorizonScanner,
        [OrganisationType.Strategic] = UserType.StrategicUser,
    };

    private readonly UserFaker _userFaker = new UserFaker();
    private readonly UserOrgMembershipStatus[] _statuses =
        Enum.GetValues<UserOrgMembershipStatus>();
    private readonly SyntheticDataSet _dataSet;
    private readonly SeedReferenceData _referenceData;

    public SeedingDataPayloadFaker()
        : this(SyntheticDataLoader.DataSet) { }

    public SeedingDataPayloadFaker(SyntheticDataSet dataSet)
    {
        _dataSet = dataSet;
        _referenceData = SeedReferenceData.Create(dataSet.ReferenceData);

        RuleFor(x => x.ReferenceData, _ => [.. _referenceData.GetAllEntities()]);
        RuleFor(x => x.Organisations, _ => CreateOrganisations());
        RuleFor(x => x.Users, (f, o) => FakeUsers(f, o));
        RuleFor(x => x.Memberships, (f, o) => FakeMemberships(f, o));
        RuleFor(x => x.SeededRecords, (_, o) => CreateRecords(o));
    }

    private Organisation[] CreateOrganisations() =>
        [
            .. _dataSet
                .Organisations.OrderBy(o => Array.IndexOf(_organisationOrder, o.OrganisationType))
                .Select(o => new Organisation
                {
                    OrganisationName = o.OrganisationName,
                    OrganisationType = o.OrganisationType,
                    AllowedPharmaceuticalEntity = o.AllowedPharmaceuticalEntity,
                    CountryOrRegion = o.CountryOrRegion,
                    HeadOfficeAddress = o.HeadOfficeAddress,
                    HeadOfficeTelephone = o.HeadOfficeTelephone,
                    HeadOfficeEmail = o.HeadOfficeEmail,
                    Status = o.Status,
                    CreatedAt = ToUtc(o.CreatedAt),
                    LastActive = o.LastActive is { } lastActive ? ToUtc(lastActive) : null,
                }),
        ];

    /// <summary>Creates users of each organisation's type, with email addresses at its domain.</summary>
    private User[] FakeUsers(Faker faker, SeedingDataPayload payload)
    {
        HashSet<string> emails = new(StringComparer.OrdinalIgnoreCase);
        return
        [
            .. payload.Organisations.SelectMany(organisation =>
            {
                string domain = organisation.HeadOfficeEmail.Split('@')[1];
                return _userFaker
                    .Generate(UsersPerOrganisation)
                    .Select(user =>
                    {
                        user.UserType = _userTypes[organisation.OrganisationType];
                        user.WorkEmail = UniqueEmail(faker, user.FullName, domain, emails);
                        // Users join soon after their organisation, before they edit any records.
                        user.CreatedAt = organisation.CreatedAt.AddDays(faker.Random.Int(0, 3));
                        user.UpdatedAt = faker.Random.Bool(0.5f)
                            ? faker.Date.Between(user.CreatedAt, DateTime.UtcNow)
                            : null;
                        user.LastActive = faker.Random.Bool(0.8f)
                            ? faker.Date.Between(user.CreatedAt, DateTime.UtcNow)
                            : null;
                        return user;
                    });
            }),
        ];
    }

    private static string UniqueEmail(
        Faker faker,
        string fullName,
        string domain,
        HashSet<string> emails
    )
    {
        string[] names = fullName.Split(' ');
        string email = faker.Internet.Email(names[0], names[^1], domain);
        int suffix = 1;
        while (!emails.Add(email))
        {
            suffix++;
            email = faker.Internet.Email(
                names[0],
                names[^1],
                domain,
                suffix.ToString(CultureInfo.InvariantCulture)
            );
        }
        return email;
    }

    private UserOrgMembership[] FakeMemberships(Faker faker, SeedingDataPayload payload)
    {
        Organisation[] organisations = [.. payload.Organisations];
        User[] users = [.. payload.Users];
        List<UserOrgMembership> memberships = [];
        for (int orgIndex = 0; orgIndex < organisations.Length; orgIndex++)
        {
            Organisation organisation = organisations[orgIndex];
            for (int userIndex = 0; userIndex < UsersPerOrganisation; userIndex++)
            {
                User user = users[(orgIndex * UsersPerOrganisation) + userIndex];
                memberships.Add(
                    new UserOrgMembership
                    {
                        User = user,
                        Organisation = organisation,
                        // Cycle through every status at least once per organisation for variety.
                        Status = _statuses[userIndex % _statuses.Length],
                        UserRole = RoleFor(faker, organisation, userIndex),
                        AllowedPharmaceuticalEntity = organisation.AllowedPharmaceuticalEntity,
                        CreatedAt = user.CreatedAt,
                    }
                );
            }
        }

        // This membership is added to enable mock auth access.
        User devUser = new UserFaker()
            .RuleFor(
                x => x.CognitoUsername,
                _ => DevAuthenticationClaims.DefaultUserCognitoUsername
            )
            .RuleFor(x => x.WorkEmail, _ => DevAuthenticationClaims.DefaultUserEmail)
            .RuleFor(x => x.UserType, _ => UserType.ItAdmin)
            .Generate();
        memberships.Add(
            new UserOrgMembership
            {
                User = devUser,
                Organisation = organisations[0],
                UserRole = UserRole.Super,
                Status = UserOrgMembershipStatus.Active,
                AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
                CreatedAt = organisations[0].CreatedAt,
            }
        );
        return [.. memberships];
    }

    /// <summary>Internal QA organisations have one super user; other organisations have a champion.</summary>
    private static UserRole RoleFor(Faker faker, Organisation organisation, int userIndex)
    {
        if (userIndex == 0)
        {
            return organisation.OrganisationType == OrganisationType.Internal
                ? UserRole.Super
                : UserRole.Champion;
        }
        return faker.Random.Bool(0.25f) ? UserRole.Champion : UserRole.Standard;
    }

    private SeededRecord[] CreateRecords(SeedingDataPayload payload)
    {
        Dictionary<string, Organisation> organisations = payload.Organisations.ToDictionary(
            o => o.OrganisationName,
            StringComparer.Ordinal
        );
        ILookup<Organisation, User> activeMembers = payload
            .Memberships.Where(m => m.Status == UserOrgMembershipStatus.Active)
            .ToLookup(m => m.Organisation!, m => m.User!);
        User[] reviewers =
        [
            .. payload
                .Organisations.Where(o => o.OrganisationType == OrganisationType.Internal)
                .SelectMany(o => activeMembers[o])
                .Where(u => u.UserType == UserType.QaUser),
        ];

        RecordHistoryFactory factory = new(new RecordContentFactory(_referenceData), _dataSet.AsOf);
        return
        [
            .. _dataSet.Records.Select(record =>
            {
                Organisation organisation = organisations[record.OrganisationName];
                return factory.Create(
                    record,
                    organisation,
                    [.. activeMembers[organisation]],
                    reviewers
                );
            }),
        ];
    }

    private static DateTime ToUtc(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}
