using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.RecordWorkflow;

namespace UKPS.Data.Configurations.RecordWorkflow;

public class RecordConfiguration : IEntityTypeConfiguration<Record>
{
    public void Configure(EntityTypeBuilder<Record> builder)
    {
        builder.ToTable("record", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordType).HasConversion<string>();
        builder.Property(x => x.RecordStatus).HasConversion<string>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ReviewedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.OrganisationId)
               .HasDatabaseName("ix_record_organisation_id");

        // Composite index supporting the review reminder scheduling job:
        // medicine + active -> reviewed_at + 3 months
        // medicine + on_hold -> reviewed_at + 6 months
        // vaccine + active -> reviewed_at + 6 months
        builder.HasIndex(x => new { x.RecordType, x.RecordStatus, x.ReviewedAt })
               .HasDatabaseName("ix_record_type_status_reviewed_at");

        builder.HasOne(x => x.Organisation)
               .WithMany()
               .HasForeignKey(x => x.OrganisationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        // Both revision FKs are nullable to allow Record to be inserted before
        // the first RecordRevision row is created. Restrict prevents cascade cycles.
        builder.HasOne(x => x.PublishedRevision)
               .WithMany()
               .HasForeignKey(x => x.PublishedRevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CurrentDraftRevision)
               .WithMany()
               .HasForeignKey(x => x.CurrentDraftRevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordRevisionConfiguration : IEntityTypeConfiguration<RecordRevision>
{
    public void Configure(EntityTypeBuilder<RecordRevision> builder)
    {
        builder.ToTable("record_revision", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.WorkflowStatus).HasConversion<string>();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.SubmittedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => new { x.RecordId, x.RevisionNo })
               .IsUnique()
               .HasDatabaseName("ix_record_revision_record_id_revision_no");

        builder.HasIndex(x => new { x.RecordId, x.MajorVersion, x.MinorVersion })
               .IsUnique()
               .HasDatabaseName("ix_record_revision_record_id_major_minor");

        // Circular relationship with Record — both sides use Restrict to avoid
        // cascade cycles. Record is always inserted first with null revision FKs.
        builder.HasOne(x => x.Record)
               .WithMany(x => x.Revisions)
               .HasForeignKey(x => x.RecordId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BasedOnRevision)
               .WithMany(x => x.DerivedRevisions)
               .HasForeignKey(x => x.BasedOnRevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
               .WithMany()
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmittedByUser)
               .WithMany()
               .HasForeignKey(x => x.SubmittedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class QaReviewConfiguration : IEntityTypeConfiguration<QaReview>
{
    public void Configure(EntityTypeBuilder<QaReview> builder)
    {
        builder.ToTable("qa_review", "ukps");
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

public class RecordStatusHistoryConfiguration : IEntityTypeConfiguration<RecordStatusHistory>
{
    public void Configure(EntityTypeBuilder<RecordStatusHistory> builder)
    {
        builder.ToTable("record_status_history", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.FromStatus).HasConversion<string>();
        builder.Property(x => x.ToStatus).HasConversion<string>();
        builder.Property(x => x.Reason).HasConversion<string>();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.RecordId)
               .HasDatabaseName("ix_record_status_history_record_id");

        builder.HasOne(x => x.Record)
               .WithMany(x => x.StatusHistory)
               .HasForeignKey(x => x.RecordId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedByUser)
               .WithMany()
               .HasForeignKey(x => x.UpdatedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

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

public class RecordEventFieldChangeConfiguration : IEntityTypeConfiguration<RecordEventFieldChange>
{
    public void Configure(EntityTypeBuilder<RecordEventFieldChange> builder)
    {
        builder.ToTable("record_event_field_change", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.FieldPath).IsRequired();

        builder.HasIndex(x => x.RecordEventId)
               .HasDatabaseName("ix_record_event_field_change_record_event_id");

        builder.HasOne(x => x.RecordEvent)
               .WithMany(x => x.FieldChanges)
               .HasForeignKey(x => x.RecordEventId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
