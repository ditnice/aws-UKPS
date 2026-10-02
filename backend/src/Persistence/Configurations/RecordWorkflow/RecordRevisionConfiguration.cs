using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Configurations.RecordWorkflow;

internal sealed class RecordRevisionConfiguration : IEntityTypeConfiguration<RecordRevision>
{
    public void Configure(EntityTypeBuilder<RecordRevision> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.WorkflowStatus);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(x => x.SubmittedAt).HasColumnType("timestamptz");

        builder
            .HasIndex(x => new { x.RecordId, x.RevisionNo })
            .IsUnique()
            .HasDatabaseName("ix_record_revision_record_id_revision_no");

        builder
            .HasOne(x => x.Record)
            .WithMany(x => x.Revisions)
            .HasForeignKey(x => x.RecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.BasedOnRevision)
            .WithMany(x => x.DerivedRevisions)
            .HasForeignKey(x => x.BasedOnRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.SubmittedByUser)
            .WithMany()
            .HasForeignKey(x => x.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
