using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Data.Seeding.SyntheticData;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Data.Seeding;

internal sealed record SeedingDataPayload
{
    public SeedReferenceData? ReferenceData { get; init; }
    public IReadOnlyCollection<Organisation> Organisations { get; init; } = [];
    public IReadOnlyCollection<User> Users { get; init; } = [];
    public IReadOnlyCollection<UserOrgMembership> Memberships { get; init; } = [];
    public IReadOnlyCollection<SeededRecord> SeededRecords { get; init; } = [];

    public IReadOnlyCollection<Record> Records => [.. SeededRecords.Select(x => x.Record)];

    /// <summary>
    /// Saves the payload in one transaction. The field audits go last: their reference data
    /// answers are resolved to IDs, which only exist once the reference data is saved.
    /// </summary>
    public async Task SaveAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        dbContext.AddRange(
            (ReferenceData?.GetAllEntities() ?? [])
                .Concat(Organisations)
                .Concat(Users)
                .Concat(Memberships)
                .Concat(Records)
                .Concat(SeededRecords.SelectMany(x => x.RevisionContent))
        );
        await dbContext.SaveChangesAsync(cancellationToken);

        RecordFieldAudit[] audits = [.. SeededRecords.SelectMany(x => x.FieldAudits)];
        if (ReferenceData is not null)
        {
            foreach (RecordFieldAudit audit in audits)
            {
                audit.OldValue = ReferenceData.ResolveAuditValue(audit.FieldPath, audit.OldValue);
                audit.NewValue = ReferenceData.ResolveAuditValue(audit.FieldPath, audit.NewValue);
            }
        }
        dbContext.AddRange(audits);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
