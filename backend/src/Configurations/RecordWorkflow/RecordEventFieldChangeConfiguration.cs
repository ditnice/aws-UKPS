using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.RecordWorkflow;

namespace UKPS.Api.Configurations.RecordWorkflow;

internal sealed class RecordEventFieldChangeConfiguration : IEntityTypeConfiguration<RecordEventFieldChange>
{
    public void Configure(EntityTypeBuilder<RecordEventFieldChange> builder)
    {
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
