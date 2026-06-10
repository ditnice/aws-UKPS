using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.MedicinesRevisionContent;

namespace UKPS.Data.Configurations.MedicinesRevisionContent;

public class MedicinesBudgetImpactConfiguration : IEntityTypeConfiguration<MedicinesBudgetImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesBudgetImpact> builder)
    {
        builder.ToTable("medicines_budget_impact", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IndicationSpecificPricingPlanned).HasConversion<string>();
        builder.Property(x => x.NetUkBudgetImpactOver5M).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_budget_impact_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
