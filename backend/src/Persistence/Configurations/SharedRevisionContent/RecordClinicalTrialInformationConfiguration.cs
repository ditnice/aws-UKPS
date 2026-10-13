using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;

namespace UKPS.Api.Persistence.Configurations.SharedRevisionContent;

internal sealed class RecordClinicalTrialInformationConfiguration
    : IEntityTypeConfiguration<RecordClinicalTrialInformation>
{
    public void Configure(EntityTypeBuilder<RecordClinicalTrialInformation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecruitingInUk);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_record_clinical_trial_information_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
