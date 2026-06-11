using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

internal sealed class RecordMhraDateConfiguration : IEntityTypeConfiguration<RecordMhraDate>
{
    public void Configure(EntityTypeBuilder<RecordMhraDate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_mhra_date_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkSubmissionDate)
               .WithMany()
               .HasForeignKey(x => x.UkSubmissionDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkLicenceDate)
               .WithMany()
               .HasForeignKey(x => x.UkLicenceDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkLaunchDate)
               .WithMany()
               .HasForeignKey(x => x.UkLaunchDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
