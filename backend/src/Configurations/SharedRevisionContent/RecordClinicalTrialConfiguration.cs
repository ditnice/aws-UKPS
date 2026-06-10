using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.SharedRevisionContent;

namespace UKPS.Data.Configurations.SharedRevisionContent;

public class RecordClinicalTrialConfiguration : IEntityTypeConfiguration<RecordClinicalTrial>
{
    public void Configure(EntityTypeBuilder<RecordClinicalTrial> builder)
    {
        builder.ToTable("record_clinical_trial", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.StudyName).IsRequired();
        builder.Property(x => x.RecruitingInUk).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId)
               .HasDatabaseName("ix_record_clinical_trial_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
