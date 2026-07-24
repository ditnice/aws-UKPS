using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;

namespace UKPS.Api.Persistence.Configurations.SharedRevisionContent;

internal sealed class OtherClinicalTrialNumberConfiguration
    : IEntityTypeConfiguration<OtherClinicalTrialNumber>
{
    public void Configure(EntityTypeBuilder<OtherClinicalTrialNumber> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.OtherRegistryNumber).IsRequired();

        builder
            .HasIndex(x => x.ClinicalTrialId)
            .HasDatabaseName("ix_other_clinical_trial_number_clinical_trial_id");

        builder
            .HasOne(x => x.ClinicalTrial)
            .WithMany(x => x.OtherClinicalTrialNumbers)
            .HasForeignKey(x => x.ClinicalTrialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
