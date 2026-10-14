using System.Text.Json;
using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class DataSeederInMemory : IDataSeeder
{
    // Champion and Standard users are members of several (but not all) pharmaceutical
    // organisations so that organisation selection can be exercised locally.
    internal const int NonSuperUserOrganisationCount = 4;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SeedDataWriter _writer;

    public DataSeederInMemory(SeedDataWriter writer)
    {
        _writer = writer;
    }

    public Task SeedData(SeedingOptions configuration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        SeedingDataPayload payload = BuildPayload(configuration);
        return _writer.Write(payload, cancellationToken);
    }

    internal static SeedingDataPayload BuildPayload(SeedingOptions configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Faker<SeedingDataPayload> faker = new SeedingDataPayloadFaker().UseSeed(0);
        SeedingDataPayload payload = faker.Generate();
        return AddConfiguredUsers(payload, configuration.SeedUsersJson);
    }

    private static SeedingDataPayload AddConfiguredUsers(
        SeedingDataPayload payload,
        string? seedUsersJson
    )
    {
        if (string.IsNullOrWhiteSpace(seedUsersJson))
        {
            return payload;
        }

        SeedUser[] configuredUsers = ParseConfiguredUsers(seedUsersJson);
        if (configuredUsers.Length == 0)
        {
            return payload;
        }

        ValidateConfiguredUsers(configuredUsers);

        List<Organisation> organisations = payload.Organisations.ToList();
        Organisation[] pharmaOrganisations =
        [
            .. organisations
                .Where(o => o.OrganisationType == OrganisationType.PharmaCompany)
                .Take(NonSuperUserOrganisationCount),
        ];
        if (pharmaOrganisations.Length < NonSuperUserOrganisationCount)
        {
            throw new InvalidOperationException(
                $"Configured seed users require at least {NonSuperUserOrganisationCount} seeded pharmaceutical organisations."
            );
        }

        foreach (Organisation organisation in pharmaOrganisations.Prepend(organisations[0]))
        {
            organisation.Status = UserOrgStatus.Active;
        }

        List<User> users = payload.Users.ToList();
        List<UserOrgMembership> memberships = payload.Memberships.ToList();

        foreach (SeedUser configuredUser in configuredUsers)
        {
            UpsertConfiguredUser(
                users,
                memberships,
                configuredUser,
                superUserOrganisation: organisations[0],
                pharmaOrganisations
            );
        }

        return payload with
        {
            Organisations = organisations,
            Users = users,
            Memberships = memberships,
        };
    }

    private static void UpsertConfiguredUser(
        List<User> users,
        List<UserOrgMembership> memberships,
        SeedUser configuredUser,
        Organisation superUserOrganisation,
        IReadOnlyList<Organisation> pharmaOrganisations
    )
    {
        User[] matchingUsers = users.Where(u => MatchesConfiguredUser(u, configuredUser)).ToArray();

        if (matchingUsers.Length > 1)
        {
            throw new InvalidOperationException(
                $"Seed user '{configuredUser.Email}' matches multiple generated users."
            );
        }

        UserRole role = Enum.Parse<UserRole>(configuredUser.Role, ignoreCase: true);
        UserType userType = role == UserRole.Super ? UserType.ItAdmin : UserType.PharmaUser;

        // A matching generated user may already be referenced by seeded records, so it is kept
        // but moved out of the configured user's way.
        foreach (User matchingUser in matchingUsers)
        {
            matchingUser.WorkEmail = $"replaced.{matchingUser.WorkEmail}";
            memberships.RemoveAll(m => ReferenceEquals(m.User, matchingUser));
        }

        User user = new()
        {
            CognitoUsername = CognitoUsername.Parse(configuredUser.CognitoUsername),
            FullName = configuredUser.FullName,
            WorkEmail = configuredUser.Email,
            UserType = userType,
            CreatedAt = DateTime.UtcNow,
        };
        users.Add(user);

        IEnumerable<Organisation> userOrganisations =
            role == UserRole.Super ? [superUserOrganisation] : pharmaOrganisations;
        memberships.AddRange(
            userOrganisations.Select(organisation => CreateMembership(user, role, organisation))
        );
    }

    private static UserOrgMembership CreateMembership(
        User user,
        UserRole role,
        Organisation organisation
    ) =>
        new()
        {
            User = user,
            Organisation = organisation,
            UserRole = role,
            Status = UserOrgMembershipStatus.Active,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
            CreatedAt = user.CreatedAt,
        };

    private static bool IsValidRole(string? role) =>
        Enum.GetNames<UserRole>().Contains(role, StringComparer.OrdinalIgnoreCase);

    private static SeedUser[] ParseConfiguredUsers(string seedUsersJson)
    {
        try
        {
            return JsonSerializer.Deserialize<SeedUser[]>(seedUsersJson, _jsonSerializerOptions)
                ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Seeding users must be a valid JSON array.",
                exception
            );
        }
    }

    private static void ValidateConfiguredUsers(IReadOnlyCollection<SeedUser> configuredUsers)
    {
        string[] duplicateEmails = configuredUsers
            .GroupBy(u => u.Email, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateEmails.Length > 0)
        {
            throw new InvalidOperationException(
                $"Seeding users contains duplicate emails: {string.Join(", ", duplicateEmails)}."
            );
        }

        string[] duplicateIdentityIds = configuredUsers
            .GroupBy(u => u.CognitoUsername, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateIdentityIds.Length > 0)
        {
            throw new InvalidOperationException(
                $"Seeding users contains duplicate identity IDs: {string.Join(", ", duplicateIdentityIds)}."
            );
        }

        foreach (SeedUser configuredUser in configuredUsers)
        {
            ValidateConfiguredUser(configuredUser);
        }
    }

    private static void ValidateConfiguredUser(SeedUser configuredUser)
    {
        if (string.IsNullOrWhiteSpace(configuredUser.FullName))
        {
            throw new InvalidOperationException("Seeding users must include a non-empty fullName.");
        }

        if (
            string.IsNullOrWhiteSpace(configuredUser.Email)
            || !configuredUser.Email.Contains('@', StringComparison.Ordinal)
        )
        {
            throw new InvalidOperationException(
                $"Seeding user '{configuredUser.FullName}' must include a valid email."
            );
        }

        if (
            string.IsNullOrWhiteSpace(configuredUser.CognitoUsername)
            || configuredUser.CognitoUsername.Length > 39
        )
        {
            throw new InvalidOperationException(
                $"Seeding user '{configuredUser.Email}' must include an cognitoUsername of 39 characters or fewer."
            );
        }

        if (!IsValidRole(configuredUser.Role))
        {
            throw new InvalidOperationException(
                $"Seeding user '{configuredUser.Email}' must include a valid role (Standard, Champion, or Super)."
            );
        }
    }

    private static bool MatchesConfiguredUser(User user, SeedUser configuredUser) =>
        string.Equals(user.WorkEmail, configuredUser.Email, StringComparison.OrdinalIgnoreCase)
        || string.Equals(
            user.CognitoUsername.Value,
            configuredUser.CognitoUsername,
            StringComparison.Ordinal
        );

    internal sealed record SeedUser
    {
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string CognitoUsername { get; init; }
        public required string Role { get; init; }
    }
}
