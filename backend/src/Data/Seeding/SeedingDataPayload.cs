using Microsoft.EntityFrameworkCore;
using UKPS.Api.Entities.Identity;

namespace UKPS.Api.Data.Seeding;

internal sealed record SeedingDataPayload
{
    public IReadOnlyCollection<Organisation> Organisations { get; init; } = [];
    public IReadOnlyCollection<User> Users { get; init; } = [];
    public IReadOnlyCollection<UserOrgMembership> Memberships { get; init; } = [];

    public object[] GetAllEntities()
    {
        return Organisations.Cast<object>().Concat(Users).Concat(Memberships).ToArray();
    }
}
