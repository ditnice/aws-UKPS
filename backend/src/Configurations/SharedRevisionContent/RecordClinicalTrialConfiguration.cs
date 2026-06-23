using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Entities.SharedRevisionContent;

namespace UKPS.Api.Configurations.SharedRevisionContent;

internal sealed class RecordClinicalTrialConfiguration
    : IEntityTypeConfiguration<RecordClinicalTrial>
{
    public void Configure(EntityTypeBuilder<RecordClinicalTrial> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.StudyName).IsRequired();
        builder.Property(x => x.RecruitingInUk);

        builder.HasIndex(x => x.RevisionId).HasDatabaseName("ix_record_clinical_trial_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
