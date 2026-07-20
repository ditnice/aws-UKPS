using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.figurations.MedicinesRevisionContent;

internal sealed class MedicinesPasRegionConfiguration : IEntityTypeConfiguration<MedicinesPasRegion>
{
    public void Configure(EntityTypeBuilder<MedicinesPasRegion> builder)
    {
        builder.HasKey(x => new { x.MedicinesBudgetImpactId, x.PasRegionId });

        builder
            .HasOne(x => x.MedicinesBudgetImpact)
            .WithMany(x => x.PasRegions)
            .HasForeignKey(x => x.MedicinesBudgetImpactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PasRegion)
            .WithMany()
            .HasForeignKey(x => x.PasRegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
