using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Persistence.Configurations.figurations.RecordWorkflow;

internal sealed class RecordStatusHistoryConfiguration
    : IEntityTypeConfiguration<RecordStatusHistory>
{
    public void Configure(EntityTypeBuilder<RecordStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.FromStatus);
        builder.Property(x => x.ToStatus);
        builder.Property(x => x.Reason);
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.RecordId).HasDatabaseName("ix_record_status_history_record_id");

        builder
            .HasOne(x => x.Record)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.RecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
