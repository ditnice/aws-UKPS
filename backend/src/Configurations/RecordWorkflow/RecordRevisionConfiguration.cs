using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.RecordWorkflow;

namespace UKPS.Data.Configurations.RecordWorkflow;

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
