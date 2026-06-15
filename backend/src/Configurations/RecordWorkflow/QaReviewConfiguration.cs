using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.RecordWorkflow;

namespace UKPS.Api.Configurations.RecordWorkflow;

internal sealed class QaReviewConfiguration : IEntityTypeConfiguration<QaReview>
{
    public void Configure(EntityTypeBuilder<QaReview> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Outcome).HasConversion<string>();
        builder.Property(x => x.ReviewedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.RevisionId)
               .HasDatabaseName("ix_qa_review_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany(x => x.QaReviews)
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
               .WithMany()
               .HasForeignKey(x => x.ReviewedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
