using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesCompanyInfoConfiguration
    : IEntityTypeConfiguration<VaccinesCompanyInfo>
{
    public void Configure(EntityTypeBuilder<VaccinesCompanyInfo> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsOriginatorCompany);
        builder.Property(x => x.HasBeenAcquired);
        builder.Property(x => x.HasGrantFunding);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_vaccines_company_info_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
