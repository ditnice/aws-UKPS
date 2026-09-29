using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;

namespace UKPS.Api.Persistence.Configurations.SharedRevisionContent;

internal sealed class RecordNameAndIdentifierConfiguration
    : IEntityTypeConfiguration<RecordNameAndIdentifier>
{
    public void Configure(EntityTypeBuilder<RecordNameAndIdentifier> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.NameType);

        builder
            .HasIndex(x => x.RecordProductDetailId)
            .HasDatabaseName("ix_record_name_and_identifier_product_detail_id");

        builder
            .HasOne(x => x.RecordProductDetail)
            .WithMany(x => x.NamesAndIdentifiers)
            .HasForeignKey(x => x.RecordProductDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
