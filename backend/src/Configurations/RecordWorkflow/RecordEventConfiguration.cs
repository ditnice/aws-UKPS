using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.RecordWorkflow;

namespace UKPS.Api.Configurations.RecordWorkflow;

public class RecordEventConfiguration : IEntityTypeConfiguration<RecordEvent>
{
    public void Configure(EntityTypeBuilder<RecordEvent> builder)
    {
        builder.ToTable("record_event", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EventType).HasConversion<string>();
        builder.Property(x => x.PerformedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");

        // Composite index supports horizon scanner timeline view (filter to
        // published/no_change events) and QA timeline view (filter to QA events)
        // without scanning all events for a record.
        builder.HasIndex(x => new { x.RecordId, x.EventType })
               .HasDatabaseName("ix_record_event_record_id_event_type");

        builder.HasIndex(x => x.RevisionId)
               .HasDatabaseName("ix_record_event_revision_id");

        builder.HasOne(x => x.Record)
               .WithMany(x => x.Events)
               .HasForeignKey(x => x.RecordId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Revision)
               .WithMany(x => x.Events)
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.QaReview)
               .WithMany()
               .HasForeignKey(x => x.QaReviewId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.QaReviewItem)
               .WithMany()
               .HasForeignKey(x => x.QaReviewItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PerformedByUser)
               .WithMany()
               .HasForeignKey(x => x.PerformedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
