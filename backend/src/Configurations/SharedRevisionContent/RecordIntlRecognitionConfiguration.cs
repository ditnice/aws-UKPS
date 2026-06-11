using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

public class RecordIntlRecognitionConfiguration : IEntityTypeConfiguration<RecordIntlRecognition>
{
    public void Configure(EntityTypeBuilder<RecordIntlRecognition> builder)
    {
        builder.ToTable("record_intl_recognition");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IntlConditionalApprovalAnticipated).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_intl_recognition_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IrpReferenceRegulator)
               .WithMany()
               .HasForeignKey(x => x.IrpReferenceRegulatorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IrpRoute)
               .WithMany()
               .HasForeignKey(x => x.IrpRouteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IntlSubmissionDate)
               .WithMany()
               .HasForeignKey(x => x.IntlSubmissionDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IntlLicenceDate)
               .WithMany()
               .HasForeignKey(x => x.IntlLicenceDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
