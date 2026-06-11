using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

public class RecordHtaConfiguration : IEntityTypeConfiguration<RecordHta>
{
    public void Configure(EntityTypeBuilder<RecordHta> builder)
    {
        builder.ToTable("record_hta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.HtaNiceAlignedPathway).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_hta_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
