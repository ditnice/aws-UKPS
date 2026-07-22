using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesTechnologyConfiguration : IEntityTypeConfiguration<VaccinesTechnology>
{
    public void Configure(EntityTypeBuilder<VaccinesTechnology> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.HasAdjuvant);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_vaccines_technology_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.VaccinePlatform)
            .WithMany()
            .HasForeignKey(x => x.VaccinePlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AdministrationRoute)
            .WithMany()
            .HasForeignKey(x => x.AdministrationRouteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
