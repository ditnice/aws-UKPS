using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.figurations.figurations.figurations.figurations.MedicinesRevisionContent;

internal sealed class MedicinesCompanyInfoConfiguration
    : IEntityTypeConfiguration<MedicinesCompanyInfo>
{
    public void Configure(EntityTypeBuilder<MedicinesCompanyInfo> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsOriginatorCompany);
        builder.Property(x => x.IsCoMarketed);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_medicines_company_info_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
