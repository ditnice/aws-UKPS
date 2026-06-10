using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.MedicinesRevisionContent;

namespace UKPS.Data.Configurations.MedicinesRevisionContent;

public class MedicinesPasRegionConfiguration : IEntityTypeConfiguration<MedicinesPasRegion>
{
    public void Configure(EntityTypeBuilder<MedicinesPasRegion> builder)
    {
        builder.ToTable("medicines_pas_region", "ukps");
        builder.HasKey(x => new { x.MedicinesBudgetImpactId, x.PasRegionId });

        builder.HasOne(x => x.MedicinesBudgetImpact)
               .WithMany(x => x.PasRegions)
               .HasForeignKey(x => x.MedicinesBudgetImpactId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PasRegion)
               .WithMany()
               .HasForeignKey(x => x.PasRegionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
