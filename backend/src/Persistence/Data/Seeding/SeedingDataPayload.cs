using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingDataPayload
{
    public IReadOnlyCollection<Organisation> Organisations { get; init; } = [];
    public IReadOnlyCollection<User> Users { get; init; } = [];
    public IReadOnlyCollection<UserOrgMembership> Memberships { get; init; } = [];
    public IReadOnlyCollection<Record> Records { get; init; } = [];

    public object[] GetAllEntities()
    {
        return Organisations
            .Cast<object>()
            .Concat(Users)
            .Concat(Memberships)
            .Concat(Records)
            .ToArray();
    }
}
