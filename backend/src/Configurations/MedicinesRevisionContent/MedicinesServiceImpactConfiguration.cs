using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

public class MedicinesServiceImpactConfiguration : IEntityTypeConfiguration<MedicinesServiceImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesServiceImpact> builder)
    {
        builder.ToTable("medicines_service_impact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ExistingNhsService).HasConversion<string>();
        builder.Property(x => x.CompassionateAccessAvailable).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_service_impact_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkPatientPopulationRange)
               .WithMany()
               .HasForeignKey(x => x.UkPatientPopulationRangeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
