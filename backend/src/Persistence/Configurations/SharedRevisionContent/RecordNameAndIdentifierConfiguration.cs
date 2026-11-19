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
        // rows of one list may share a position, a missing one included (nulls not distinct). Its
        // leading column also serves the foreign key.
        builder
            .HasIndex(x => new
            {
                x.RecordProductDetailId,
                x.NameType,
                x.DisplayOrder,
            })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName(ConstraintNames.NameAndIdentifierUniqueDisplayOrderPerList);

        builder
            .HasOne(x => x.RecordProductDetail)
            .WithMany(x => x.NamesAndIdentifiers)
            .HasForeignKey(x => x.RecordProductDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
