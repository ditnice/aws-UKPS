using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

public class RegulatoryDateConfiguration : IEntityTypeConfiguration<RegulatoryDate>
{
    public void Configure(EntityTypeBuilder<RegulatoryDate> builder)
    {
        builder.ToTable("regulatory_date", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DateEvent).HasConversion<string>();
        builder.Property(x => x.DatePrecision).HasConversion<string>();
        builder.Property(x => x.DateValue).IsRequired();

        builder.HasIndex(x => new { x.RevisionId, x.DateEvent, x.DatePrecision })
               .IsUnique()
               .HasDatabaseName("ix_regulatory_date_revision_event_precision");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
