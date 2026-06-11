using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

public class MedicinesActiveSubstanceConfiguration : IEntityTypeConfiguration<MedicinesActiveSubstance>
{
    public void Configure(EntityTypeBuilder<MedicinesActiveSubstance> builder)
    {
        builder.ToTable("medicines_active_substance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.NameType).HasConversion<string>();

        builder.HasIndex(x => x.MedicinesProductDetailId)
               .HasDatabaseName("ix_medicines_active_substance_product_detail_id");

        builder.HasOne(x => x.MedicinesProductDetail)
               .WithMany(x => x.ActiveSubstances)
               .HasForeignKey(x => x.MedicinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
