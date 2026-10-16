using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Configurations.RecordWorkflow;

internal sealed class RecordFieldAuditConfiguration : IEntityTypeConfiguration<RecordFieldAudit>
{
    public void Configure(EntityTypeBuilder<RecordFieldAudit> builder)
    {
        builder.ToTable("record_field_audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.FieldPath).IsRequired();
        builder.Property(x => x.ChangedAt).HasColumnType("timestamptz").IsRequired();

        // Supports compiling a diff of old value -> new value for each field. This will
        // be needed to show whats changed between each QA submission as well as between
        // each published version. If this table gets too large and these queries are too
        // slow we can look to add in a diff-specific entity.
        builder
            .HasIndex(x => new
            {
                x.RevisionId,
                x.FieldPath,
                x.ChangedAt,
            })
            .HasDatabaseName("ix_record_field_audit_revision_field_time");

        builder.HasIndex(x => x.RecordId).HasDatabaseName("ix_record_field_audit_record_id");

        builder
            .HasOne(x => x.Record)
            .WithMany()
            .HasForeignKey(x => x.RecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
