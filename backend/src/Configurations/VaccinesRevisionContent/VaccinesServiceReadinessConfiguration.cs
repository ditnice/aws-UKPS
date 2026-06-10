using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.VaccinesRevisionContent;

namespace UKPS.Data.Configurations.VaccinesRevisionContent;

public class VaccinesServiceReadinessConfiguration : IEntityTypeConfiguration<VaccinesServiceReadiness>
{
    public void Configure(EntityTypeBuilder<VaccinesServiceReadiness> builder)
    {
        builder.ToTable("vaccines_service_readiness", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RequiresReconstitution).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_service_readiness_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StorageRequirement)
               .WithMany()
               .HasForeignKey(x => x.StorageRequirementId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
