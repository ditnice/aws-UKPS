using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence.Configurations.VaccinesRevisionContent;

internal sealed class VaccinesProductDetailConfiguration
    : IEntityTypeConfiguration<VaccinesProductDetail>
{
    public void Configure(EntityTypeBuilder<VaccinesProductDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordTitle).IsRequired();
        builder.Property(x => x.CompanyCode).IsRequired();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_vaccines_product_detail_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
