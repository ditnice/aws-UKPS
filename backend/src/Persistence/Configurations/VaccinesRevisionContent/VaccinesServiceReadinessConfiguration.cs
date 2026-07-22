using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesServiceReadinessConfiguration
    : IEntityTypeConfiguration<VaccinesServiceReadiness>
{
    public void Configure(EntityTypeBuilder<VaccinesServiceReadiness> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RequiresReconstitution);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_vaccines_service_readiness_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.StorageRequirement)
            .WithMany()
            .HasForeignKey(x => x.StorageRequirementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
