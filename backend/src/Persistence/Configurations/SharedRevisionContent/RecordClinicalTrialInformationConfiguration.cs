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
        builder.Property(x => x.HasClinicalTrials);

        builder
            .HasIndex(x => x.RevisionId)
            .IsUnique()
            .HasDatabaseName("ix_record_clinical_trial_information_revision_id");

        builder
            .HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        // A revision's trials hang off its clinical trial information row, joined on the
        // revision, so the form can read and write them through this row like any child
        // collection. A trial therefore needs the information row of its revision.
        builder
            .HasMany(x => x.ClinicalTrials)
            .WithOne()
            .HasPrincipalKey(x => x.RevisionId)
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
