using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;

namespace UKPS.Api.Persistence.Configurations.SharedRevisionContent;

internal sealed class RecordProductDetailConfiguration
    : IEntityTypeConfiguration<RecordProductDetail>
{
    public void Configure(EntityTypeBuilder<RecordProductDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.CompanyCode).IsRequired();
        builder.Property(x => x.RecordTitle).IsRequired();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_record_product_detail_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
