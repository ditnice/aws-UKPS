using System.Globalization;
using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Data.Seeding.SyntheticData;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed class SeedingDataPayloadFaker : Faker<SeedingDataPayload>
{
    internal const int UsersPerOrganisation = 16;
    private const float DiscardedRecordsPerOrganisation = 2;

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

    // Removed is excluded: seeded users are never anonymised, so seeding one as Removed
    // would leave fake PII on a membership that should have none (BR-16).
    private readonly UserOrgMembershipStatus[] _statuses = Enum.GetValues<UserOrgMembershipStatus>()
        .Where(x => x != UserOrgMembershipStatus.Removed)
        .ToArray();
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
        RuleFor(x => x.SeededRecords, (f, o) => CreateRecords(f, o));
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
                        // Cycle through every non-Removed status per organisation for variety.
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

    /// <summary>
    /// Creates a record from each synthetic record, mixing in records that were discarded soon
    /// after being started: about <see cref="DiscardedRecordsPerOrganisation"/> per organisation,
    /// whatever its size.
    /// </summary>
    private SeededRecord[] CreateRecords(Faker faker, SeedingDataPayload payload)
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

        Dictionary<string, float> discardChances = _dataSet
            .Records.CountBy(r => r.OrganisationName, StringComparer.Ordinal)
            .ToDictionary(
                x => x.Key,
                x => DiscardedRecordsPerOrganisation / x.Value,
                StringComparer.Ordinal
            );

        RecordHistoryFactory factory = new(new RecordContentFactory(_referenceData), _dataSet.AsOf);
        List<SeededRecord> records = [];
        foreach (SyntheticRecord record in _dataSet.Records)
        {
            Organisation organisation = organisations[record.OrganisationName];
            User[] authors = [.. activeMembers[organisation]];
            if (faker.Random.Bool(discardChances[record.OrganisationName]))
            {
                records.Add(CreateDiscardedRecord(faker, organisation, faker.PickRandom(authors)));
            }
            records.Add(factory.Create(record, organisation, authors, reviewers));
        }
        return [.. records];
    }

    private SeededRecord CreateDiscardedRecord(Faker faker, Organisation organisation, User author)
    {
        DateTime today = ToUtc(_dataSet.AsOf);
        DateTime createdAt = faker.Date.Between(today.AddYears(-1), today.AddDays(-1));

        Record record = new()
        {
            Organisation = organisation,
            RecordType = RecordType.Medicine,
            CreatedAt = createdAt,
            CreatedByUser = author,
        };
        RecordRevision revision = new()
        {
            Record = record,
            WorkflowStatus = WorkflowStatus.Draft,
            CreatedAt = createdAt,
            CreatedByUser = author,
        };
        record.Revisions.Add(revision);
        record.StatusHistory.Add(
            new RecordStatusHistory
            {
                Record = record,
                ToStatus = RecordStatus.Unpublished,
                UpdatedAt = createdAt,
                UpdatedByUser = author,
            }
        );
        record.Events.Add(
            new RecordEvent
            {
                Record = record,
                Revision = revision,
                EventType = RecordEventType.RecordCreated,
                PerformedAt = createdAt,
                PerformedByUser = author,
            }
        );

        DiscardedDraft draft = new(faker, revision, author);
        RecordProductDetail productDetail = draft.SaveProductDetail();
        MedicinesIndicationDetail indicationDetail = draft.SaveIndicationAnswers();
        bool isDiscarded = record.TryDiscard(
            revision,
            hasSingleRevision: true,
            faker.Date.Between(draft.LastSavedAt, today),
            author,
            RecordStatusChangeReason.ManuallyDiscarded
        );
        if (!isDiscarded)
        {
            throw new InvalidOperationException("The seeded draft record could not be discarded.");
        }

        return new SeededRecord(record, [productDetail, indicationDetail], draft.Audits);
    }

    private static DateTime ToUtc(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}
