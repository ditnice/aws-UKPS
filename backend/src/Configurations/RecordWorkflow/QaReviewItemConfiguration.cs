using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.RecordWorkflow;

namespace UKPS.Data.Configurations.RecordWorkflow;

public class QaReviewItemConfiguration : IEntityTypeConfiguration<QaReviewItem>
{
    public void Configure(EntityTypeBuilder<QaReviewItem> builder)
    {
        builder.ToTable("qa_review_item", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.FieldPath).IsRequired();
        builder.Property(x => x.IssueType).HasConversion<string>();
        builder.Property(x => x.ResolutionStatus).HasConversion<string>();
        builder.Property(x => x.ResolvedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.QaReviewId)
               .HasDatabaseName("ix_qa_review_item_qa_review_id");

        builder.HasOne(x => x.QaReview)
               .WithMany(x => x.QaReviewItems)
               .HasForeignKey(x => x.QaReviewId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResolvedByUser)
               .WithMany()
               .HasForeignKey(x => x.ResolvedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
