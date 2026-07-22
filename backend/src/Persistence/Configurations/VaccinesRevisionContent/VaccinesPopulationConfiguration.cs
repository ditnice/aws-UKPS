using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesPopulationConfiguration : IEntityTypeConfiguration<VaccinesPopulation>
{
    public void Configure(EntityTypeBuilder<VaccinesPopulation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_vaccines_population_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
