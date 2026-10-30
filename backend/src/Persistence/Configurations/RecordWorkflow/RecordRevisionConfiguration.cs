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
        builder.Property(x => x.Version).IsRowVersion();

        // Maps to PostgreSQL's xmin, so an update fails if the revision changed since it was read,
        // for example by being submitted while it is being discarded.
        builder.Property(x => x.Version).IsRowVersion();

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
    }
}
