using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.MedicinesRevisionContent;

namespace UKPS.Data.Configurations.MedicinesRevisionContent;

public class MedicinesCompanyInfoConfiguration : IEntityTypeConfiguration<MedicinesCompanyInfo>
{
    public void Configure(EntityTypeBuilder<MedicinesCompanyInfo> builder)
    {
        builder.ToTable("medicines_company_info", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsOriginatorCompany).HasConversion<string>();
        builder.Property(x => x.IsCoMarketed).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_company_info_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
