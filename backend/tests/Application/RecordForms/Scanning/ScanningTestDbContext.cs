using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Persistence;

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

/// <summary>An EF model of the test-only entities, built without a database.</summary>
internal sealed class ScanningTestDbContext(DbContextOptions<ScanningTestDbContext> options)
    : DbContext(options)
{
    public static IModel CreateModel()
    {
        DbContextOptions<ScanningTestDbContext> options =
            new DbContextOptionsBuilder<ScanningTestDbContext>()
                .UseNpgsql()
                .UseSnakeCaseNamingConvention()
                .Options;
        using ScanningTestDbContext dbContext = new(options);
        return dbContext.GetService<IDesignTimeModel>().Model;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The test entities reference RecordRevision, which pulls in the app's entity graph.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<ValidEntity>();
        modelBuilder
            .Entity<ValidEntityArea>()
            .HasKey(x => new { x.ValidEntityId, x.TherapeuticAreaId });
        modelBuilder.Entity<BrokenEntity>();
        modelBuilder.Entity<RowsEntity>();
        modelBuilder.Entity<DisagreeingEntity>();
        modelBuilder.Entity<TaskConflictEntity>();
    }
}
