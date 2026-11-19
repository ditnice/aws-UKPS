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

        // Each list sharing the collection (one per name type) numbers its rows from 1, so no two
        // rows of one list may share a position. Its leading column also serves the foreign key.
        builder
            .HasIndex(x => new
            {
                x.RecordProductDetailId,
                x.NameType,
                x.DisplayOrder,
            })
            .IsUnique()
            .HasDatabaseName("ix_record_name_and_identifier_product_detail_type_display_order");

        builder
            .HasOne(x => x.RecordProductDetail)
            .WithMany(x => x.NamesAndIdentifiers)
            .HasForeignKey(x => x.RecordProductDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
