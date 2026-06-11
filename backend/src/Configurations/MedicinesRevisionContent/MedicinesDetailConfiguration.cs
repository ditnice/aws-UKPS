using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Configurations.MedicinesRevisionContent;

public class MedicinesDetailConfiguration : IEntityTypeConfiguration<MedicinesDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesDetail> builder)
    {
        builder.ToTable("medicines_detail");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsPersonalisedMedicine).HasConversion<string>();
        builder.Property(x => x.IsRepurposedMedicine).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
