using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.SharedRevisionContent;

namespace UKPS.Data.Configurations.SharedRevisionContent;

public class RegulatoryDateConfiguration : IEntityTypeConfiguration<RegulatoryDate>
{
    public void Configure(EntityTypeBuilder<RegulatoryDate> builder)
    {
        builder.ToTable("regulatory_date", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DateEvent).HasConversion<string>();
        builder.Property(x => x.DatePrecision).HasConversion<string>();
        builder.Property(x => x.DateValue).IsRequired();

        builder.HasIndex(x => new { x.RevisionId, x.DateEvent, x.DatePrecision })
               .IsUnique()
               .HasDatabaseName("ix_regulatory_date_revision_event_precision");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordMhraProcedureConfiguration : IEntityTypeConfiguration<RecordMhraProcedure>
{
    public void Configure(EntityTypeBuilder<RecordMhraProcedure> builder)
    {
        builder.ToTable("record_mhra_procedure", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_mhra_procedure_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MhraProcedureType)
               .WithMany()
               .HasForeignKey(x => x.MhraProcedureTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordMhraDateConfiguration : IEntityTypeConfiguration<RecordMhraDate>
{
    public void Configure(EntityTypeBuilder<RecordMhraDate> builder)
    {
        builder.ToTable("record_mhra_date", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_mhra_date_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkSubmissionDate)
               .WithMany()
               .HasForeignKey(x => x.UkSubmissionDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkLicenceDate)
               .WithMany()
               .HasForeignKey(x => x.UkLicenceDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkLaunchDate)
               .WithMany()
               .HasForeignKey(x => x.UkLaunchDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordIntlRecognitionConfiguration : IEntityTypeConfiguration<RecordIntlRecognition>
{
    public void Configure(EntityTypeBuilder<RecordIntlRecognition> builder)
    {
        builder.ToTable("record_intl_recognition", "ukps");
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

public class RecordGlobalSubmissionConfiguration : IEntityTypeConfiguration<RecordGlobalSubmission>
{
    public void Configure(EntityTypeBuilder<RecordGlobalSubmission> builder)
    {
        builder.ToTable("record_global_submission", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_global_submission_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GlobalSubmissionEstimatedDate)
               .WithMany()
               .HasForeignKey(x => x.GlobalSubmissionEstimatedDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GlobalSubmissionActualDate)
               .WithMany()
               .HasForeignKey(x => x.GlobalSubmissionActualDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordHtaConfiguration : IEntityTypeConfiguration<RecordHta>
{
    public void Configure(EntityTypeBuilder<RecordHta> builder)
    {
        builder.ToTable("record_hta", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.HtaNiceAlignedPathway).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_record_hta_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecordHtaBodyConfiguration : IEntityTypeConfiguration<RecordHtaBody>
{
    public void Configure(EntityTypeBuilder<RecordHtaBody> builder)
    {
        builder.ToTable("record_hta_body", "ukps");
        builder.HasKey(x => new { x.RecordHtaId, x.Label });
        builder.Property(x => x.Label).IsRequired();

        builder.HasOne(x => x.RecordHta)
               .WithMany(x => x.HtaBodies)
               .HasForeignKey(x => x.RecordHtaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

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

public class OtherClinicalTrialNumberConfiguration : IEntityTypeConfiguration<OtherClinicalTrialNumber>
{
    public void Configure(EntityTypeBuilder<OtherClinicalTrialNumber> builder)
    {
        builder.ToTable("other_clinical_trial_number", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.OtherRegistryNumber).IsRequired();

        builder.HasIndex(x => x.ClinicalTrialId)
               .HasDatabaseName("ix_other_clinical_trial_number_clinical_trial_id");

        builder.HasOne(x => x.ClinicalTrial)
               .WithMany(x => x.OtherClinicalTrialNumbers)
               .HasForeignKey(x => x.ClinicalTrialId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
