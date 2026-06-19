using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

internal sealed class MedicinesBudgetImpactConfiguration : IEntityTypeConfiguration<MedicinesBudgetImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesBudgetImpact> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IndicationSpecificPricingPlanned);
        builder.Property(x => x.NetUkBudgetImpactOver5M);

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_budget_impact_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
