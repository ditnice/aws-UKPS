using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Persistence;

/// <summary>
/// Checks the reference data seeded by migrations. The shared test database is emptied between
/// tests, so these run against a freshly migrated database of their own.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ReferenceDataSeedTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly string _database = $"seed_check_{Guid.NewGuid():N}";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using var context = fixture.CreateContext();
        // Database names cannot be parameters; this one is generated above.
        var createDatabase = "CREATE DATABASE " + _database;
        await context.Database.ExecuteSqlRawAsync(createDatabase, Ct);
        await using var seeded = fixture.CreateContext(_database);
        await seeded.Database.MigrateAsync(Ct);
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = fixture.CreateContext();
        var dropDatabase = "DROP DATABASE " + _database + " WITH (FORCE)";
        await context.Database.ExecuteSqlRawAsync(dropDatabase, CancellationToken.None);
    }

    [Fact]
    public async Task Migrations_SeedTheFifteenBnfChaptersAsTopLevelNodes()
    {
        await using var context = fixture.CreateContext(_database);

        var chapters = await context.BnfChapters.OrderBy(x => x.DisplayOrder).ToListAsync(Ct);

        chapters.Count.ShouldBe(15);
        chapters.ShouldAllBe(x => x.ParentId == null && !x.IsArchived);
        chapters.Select(x => x.Code).ShouldBe(Enumerable.Range(1, 15).Select(i => $"{i}"));
        chapters[0].Label.ShouldBe("Gastro-intestinal system");
        chapters[^1].Label.ShouldBe("Anaesthesia");
    }

    [Fact]
    public async Task Migrations_SeedTheTwentyTherapeuticAreasInOrder()
    {
        await using var context = fixture.CreateContext(_database);

        var areas = await context.TherapeuticAreas.OrderBy(x => x.DisplayOrder).ToListAsync(Ct);

        areas.Count.ShouldBe(20);
        areas.ShouldAllBe(x => x.ParentId == null && !x.IsArchived);
        areas
            .Select(x => x.Label)
            .ShouldAllBe(label => label.Contains(": ", StringComparison.Ordinal));
        areas[0].Label.ShouldBe("1: Addiction");
        areas[^1].Label.ShouldBe("20: Urologic");
    }

    [Fact]
    public async Task Migrations_MoveIdentitySequencesPastTheSeededRows()
    {
        await using var context = fixture.CreateContext(_database);
        var chapter = new BnfChapter { Code = "99", Label = "New chapter" };
        var area = new TherapeuticArea { Label = "21: New area" };
        context.AddRange(chapter, area);

        await context.SaveChangesAsync(Ct);

        chapter.Id.ShouldBe(16);
        area.Id.ShouldBe(21);
    }
}
