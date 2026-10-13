using UKPS.Api.Persistence.Data.Seeding.SyntheticData;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingDataPayload
{
    public IReadOnlyCollection<object> ReferenceData { get; init; } = [];
    public IReadOnlyCollection<Organisation> Organisations { get; init; } = [];
    public IReadOnlyCollection<User> Users { get; init; } = [];
    public IReadOnlyCollection<UserOrgMembership> Memberships { get; init; } = [];
    public IReadOnlyCollection<SeededRecord> SeededRecords { get; init; } = [];

    public IReadOnlyCollection<Record> Records => [.. SeededRecords.Select(x => x.Record)];

    public object[] GetAllEntities()
    {
        return ReferenceData
            .Concat(Organisations)
            .Concat(Users)
            .Concat(Memberships)
            .Concat(Records)
            .Concat(SeededRecords.SelectMany(x => x.RevisionContent))
            .ToArray();
    }
}
