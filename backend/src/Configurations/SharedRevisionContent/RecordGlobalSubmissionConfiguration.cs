using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

internal sealed class RecordGlobalSubmissionConfiguration
    : IEntityTypeConfiguration<RecordGlobalSubmission>
{
    public void Configure(EntityTypeBuilder<RecordGlobalSubmission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_record_global_submission_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.GlobalSubmissionEstimatedDate)
            .WithMany()
            .HasForeignKey(x => x.GlobalSubmissionEstimatedDateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.GlobalSubmissionActualDate)
            .WithMany()
            .HasForeignKey(x => x.GlobalSubmissionActualDateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
