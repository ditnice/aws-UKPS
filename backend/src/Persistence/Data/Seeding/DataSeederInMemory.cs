using System.Text.Json;
using Bogus;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class DataSeederInMemory : IDataSeeder
{
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
        if (organisations.Count == 0)
        {
            throw new InvalidOperationException(
                "Configured seed users require seeded organisation ID 1."
            );
        }

        organisations[0].Status = UserOrgStatus.Active;

        List<User> users = payload.Users.ToList();
        List<UserOrgMembership> memberships = payload.Memberships.ToList();

        foreach (SeedUser configuredUser in configuredUsers)
        {
            UpsertConfiguredUser(users, memberships, configuredUser);
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
        SeedUser configuredUser
    )
    {
        User[] matchingUsers = users.Where(u => MatchesConfiguredUser(u, configuredUser)).ToArray();

        if (matchingUsers.Length > 1)
        {
            throw new InvalidOperationException(
                $"Seed user '{configuredUser.Email}' matches multiple generated users."
            );
        }

        foreach (User matchingUser in matchingUsers)
        {
            users.Remove(matchingUser);
            memberships.RemoveAll(m => ReferenceEquals(m.User, matchingUser));
        }

        UserRole role = Enum.Parse<UserRole>(configuredUser.Role, ignoreCase: true);
        User user = CreateConfiguredUser(configuredUser, role);
        users.Add(user);
        memberships.Add(CreateMembership(user, role));
    }

    private static User CreateConfiguredUser(SeedUser configuredUser, UserRole role) =>
        new()
        {
            CognitoUsername = CognitoUsername.Parse(configuredUser.CognitoUsername),
            FullName = configuredUser.FullName,
            WorkEmail = configuredUser.Email,
            UserType = role == UserRole.Super ? UserType.ItAdmin : UserType.PharmaUser,
            CreatedAt = DateTime.UtcNow,
        };

    private static UserOrgMembership CreateMembership(User user, UserRole role) =>
        new()
        {
            User = user,
            OrganisationId = 1,
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
