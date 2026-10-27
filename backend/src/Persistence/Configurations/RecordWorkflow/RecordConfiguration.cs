using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Configurations.RecordWorkflow;

internal sealed class RecordConfiguration : IEntityTypeConfiguration<Record>
{
    /// <summary>
    /// The name of the query filter that hides discarded records. Queries that need discarded
    /// records opt out with <c>IgnoreQueryFilters([RecordConfiguration.DiscardedFilter])</c>.
    /// </summary>
    internal const string DiscardedFilter = "Discarded";

    public void Configure(EntityTypeBuilder<Record> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordType);
        builder.Property(x => x.RecordStatus);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ReviewedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.OrganisationId).HasDatabaseName("ix_record_organisation_id");

        // Composite index supporting the review reminder scheduling job:
        // medicine + active -> reviewed_at + 3 months
        // medicine + on_hold -> reviewed_at + 6 months
        // vaccine + active -> reviewed_at + 6 months
        builder
            .HasIndex(x => new
            {
                x.RecordType,
                x.RecordStatus,
                x.ReviewedAt,
            })
            .HasDatabaseName("ix_record_type_status_reviewed_at");

        // Discarded records are hidden everywhere. Only queries that start from records are
        // filtered: revisions, events and other tables still include those of discarded records.
        builder.HasQueryFilter(DiscardedFilter, x => x.RecordStatus != RecordStatus.Discarded);

        builder
            .HasOne(x => x.Organisation)
            .WithMany()
            .HasForeignKey(x => x.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
