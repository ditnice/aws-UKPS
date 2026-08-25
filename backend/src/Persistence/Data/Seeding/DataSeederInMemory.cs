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
        return AddConfiguredSuperUsers(payload, configuration.SuperUsersJson);
    }

    private static SeedingDataPayload AddConfiguredSuperUsers(
        SeedingDataPayload payload,
        string? superUsersJson
    )
    {
        if (string.IsNullOrWhiteSpace(superUsersJson))
        {
            return payload;
        }

        SeedSuperUser[] configuredUsers = ParseConfiguredUsers(superUsersJson);
        if (configuredUsers.Length == 0)
        {
            return payload;
        }

        ValidateConfiguredUsers(configuredUsers);

        List<Organisation> organisations = payload.Organisations.ToList();
        if (organisations.Count == 0)
        {
            throw new InvalidOperationException(
                "Configured seed super users require seeded organisation ID 1."
            );
        }

        organisations[0].Status = UserOrgStatus.Active;

        List<User> users = payload.Users.ToList();
        List<UserOrgMembership> memberships = payload.Memberships.ToList();

        foreach (SeedSuperUser configuredUser in configuredUsers)
        {
            UpsertConfiguredSuperUser(users, memberships, configuredUser);
        }

        return payload with
        {
            Organisations = organisations,
            Users = users,
            Memberships = memberships,
        };
    }

    private static void UpsertConfiguredSuperUser(
        List<User> users,
        List<UserOrgMembership> memberships,
        SeedSuperUser configuredUser
    )
    {
        User[] matchingUsers = users.Where(u => MatchesConfiguredUser(u, configuredUser)).ToArray();

        if (matchingUsers.Length > 1)
        {
            throw new InvalidOperationException(
                $"Seed super user '{configuredUser.Email}' matches multiple generated users."
            );
        }

        foreach (User matchingUser in matchingUsers)
        {
            users.Remove(matchingUser);
            memberships.RemoveAll(m => ReferenceEquals(m.User, matchingUser));
        }

        User user = CreateConfiguredSuperUser(configuredUser);
        users.Add(user);
        memberships.Add(CreateSuperUserMembership(user));
    }

    private static User CreateConfiguredSuperUser(SeedSuperUser configuredUser) =>
        new()
        {
            CognitoUsername = CognitoUsername.Parse(configuredUser.CognitoUsername),
            FullName = configuredUser.FullName,
            WorkEmail = configuredUser.Email,
            UserType = UserType.ItAdmin,
            CreatedAt = DateTime.UtcNow,
        };

    private static UserOrgMembership CreateSuperUserMembership(User user) =>
        new()
        {
            User = user,
            OrganisationId = 1,
            UserRole = UserRole.Super,
            Status = UserOrgStatus.Active,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
            CreatedAt = user.CreatedAt,
        };

    private static SeedSuperUser[] ParseConfiguredUsers(string superUsersJson)
    {
        try
        {
            return JsonSerializer.Deserialize<SeedSuperUser[]>(
                    superUsersJson,
                    _jsonSerializerOptions
                ) ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Seeding super users must be a valid JSON array.",
                exception
            );
        }
    }

    private static void ValidateConfiguredUsers(IReadOnlyCollection<SeedSuperUser> configuredUsers)
    {
        string[] duplicateEmails = configuredUsers
            .GroupBy(u => u.Email, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateEmails.Length > 0)
        {
            throw new InvalidOperationException(
                $"Seeding super users contains duplicate emails: {string.Join(", ", duplicateEmails)}."
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
                $"Seeding super users contains duplicate identity IDs: {string.Join(", ", duplicateIdentityIds)}."
            );
        }

        foreach (SeedSuperUser configuredUser in configuredUsers)
        {
            if (string.IsNullOrWhiteSpace(configuredUser.FullName))
            {
                throw new InvalidOperationException(
                    "Seeding super users must include a non-empty fullName."
                );
            }

            if (
                string.IsNullOrWhiteSpace(configuredUser.Email)
                || !configuredUser.Email.Contains('@', StringComparison.Ordinal)
            )
            {
                throw new InvalidOperationException(
                    $"Seeding super user '{configuredUser.FullName}' must include a valid email."
                );
            }

            if (
                string.IsNullOrWhiteSpace(configuredUser.CognitoUsername)
                || configuredUser.CognitoUsername.Length > 39
            )
            {
                throw new InvalidOperationException(
                    $"Seeding super user '{configuredUser.Email}' must include an cognitoUsername of 39 characters or fewer."
                );
            }
        }
    }

    private static bool MatchesConfiguredUser(User user, SeedSuperUser configuredUser) =>
        string.Equals(user.WorkEmail, configuredUser.Email, StringComparison.OrdinalIgnoreCase)
        || string.Equals(
            user.CognitoUsername.Value,
            configuredUser.CognitoUsername,
            StringComparison.Ordinal
        );

    internal sealed record SeedSuperUser
    {
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string CognitoUsername { get; init; }
    }
}
