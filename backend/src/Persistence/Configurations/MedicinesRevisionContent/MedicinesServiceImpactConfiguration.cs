using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesServiceImpactConfiguration
    : IEntityTypeConfiguration<MedicinesServiceImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesServiceImpact> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ExistingNhsService);
        builder.Property(x => x.CompassionateAccessAvailable);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_service_impact_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.UkPatientPopulationRange)
            .WithMany()
            .HasForeignKey(x => x.UkPatientPopulationRangeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
