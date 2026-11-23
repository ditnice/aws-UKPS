using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Shouldly;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class NameAndIdentifierDisplayOrderMigrationTests(PostgresFixture fixture)
    : DatabaseTestBase(fixture)
{
    private const string MigrationBeforeUniqueIndex =
        "20261118154040_AddingXminVersionColumnForUserConcurrencyCheck";

    [Fact]
    public async Task Up_RenumbersEachListFromOneInTheOrderTheFormReadsIt()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        RecordProductDetail productDetail = await AddProductDetail(ct);
        RecordNameAndIdentifier[] generic =
        [
            .. Enumerable
                .Range(1, 4)
                .Select(i =>
                    Name(productDetail, $"generic {i}", NameAndIdentifierType.GenericName, i)
                ),
        ];
        RecordNameAndIdentifier[] other =
        [
            .. Enumerable
                .Range(1, 2)
                .Select(i =>
                    Name(productDetail, $"other {i}", NameAndIdentifierType.OtherIdentifier, i)
                ),
        ];
        await AddEntities([.. generic, .. other], ct);
        Context.ChangeTracker.Clear();

        IMigrator migrator = Context.GetService<IMigrator>();
        await migrator.MigrateAsync(MigrationBeforeUniqueIndex, ct);
        try
        {
            // Without the index, give the generic names a gap, a repeat and a missing position,
            // and the other identifiers none at all.
            await SetDisplayOrder(generic[0], 2, ct);
            await SetDisplayOrder(generic[1], null, ct);
            await SetDisplayOrder(generic[2], 2, ct);
            await SetDisplayOrder(generic[3], 1, ct);
            await SetDisplayOrder(other[0], null, ct);
            await SetDisplayOrder(other[1], null, ct);

            await migrator.MigrateAsync(cancellationToken: ct);

            Dictionary<int, int?> displayOrders = await Context
                .RecordNamesAndIdentifiers.AsNoTracking()
                .ToDictionaryAsync(static n => n.Id, static n => n.DisplayOrder, ct);
            // By display order with none last, then ID.
            generic.Select(n => displayOrders[n.Id]).ShouldBe([2, 4, 3, 1]);
            other.Select(n => displayOrders[n.Id]).ShouldBe([1, 2]);
        }
        finally
        {
            // Leave the shared test database at the latest schema even if an assertion fails.
            await migrator.MigrateAsync(cancellationToken: ct);
        }
    }

    private async Task SetDisplayOrder(
        RecordNameAndIdentifier name,
        int? displayOrder,
        CancellationToken ct
    )
    {
        int updated = await Context
            .RecordNamesAndIdentifiers.Where(n => n.Id == name.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.DisplayOrder, displayOrder), ct);
        updated.ShouldBe(1);
    }

    private async Task<RecordProductDetail> AddProductDetail(CancellationToken ct)
    {
        Organisation organisation = await AddEntity(new OrganisationFaker().Generate(), ct);
        User user = await AddEntity(new UserFaker().Generate(), ct);
        Record record = await AddEntity(
            new RecordFaker().RuleFor(x => x.OrganisationId, organisation.Id).Generate(),
            ct
        );
        RecordRevision revision = await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, user.Id)
                .Generate(),
            ct
        );
        return await AddEntity(
            new RecordProductDetailFaker(RecordType.Medicine, revision.Id).Generate(),
            ct
        );
    }

    private static RecordNameAndIdentifier Name(
        RecordProductDetail productDetail,
        string name,
        NameAndIdentifierType type,
        int displayOrder
    ) =>
        new()
        {
            RecordProductDetailId = productDetail.Id,
            Name = name,
            NameType = type,
            DisplayOrder = displayOrder,
        };
}
