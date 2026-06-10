using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.MedicinesRevisionContent;

namespace UKPS.Data.Configurations.MedicinesRevisionContent;

public class MedicinesProductDetailConfiguration : IEntityTypeConfiguration<MedicinesProductDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesProductDetail> builder)
    {
        builder.ToTable("medicines_product_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordTitle).IsRequired();
        builder.Property(x => x.Indication).IsRequired();
        builder.Property(x => x.IndicationIsPaediatric).HasConversion<string>();
        builder.Property(x => x.IndicationIsCancer).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_product_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BnfChapter)
               .WithMany()
               .HasForeignKey(x => x.BnfChapterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TherapeuticArea)
               .WithMany()
               .HasForeignKey(x => x.TherapeuticAreaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FormulationType)
               .WithMany()
               .HasForeignKey(x => x.FormulationTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesActiveSubstanceConfiguration : IEntityTypeConfiguration<MedicinesActiveSubstance>
{
    public void Configure(EntityTypeBuilder<MedicinesActiveSubstance> builder)
    {
        builder.ToTable("medicines_active_substance", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.NameType).HasConversion<string>();

        builder.HasIndex(x => x.MedicinesProductDetailId)
               .HasDatabaseName("ix_medicines_active_substance_product_detail_id");

        builder.HasOne(x => x.MedicinesProductDetail)
               .WithMany(x => x.ActiveSubstances)
               .HasForeignKey(x => x.MedicinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesRecordStatusConfiguration : IEntityTypeConfiguration<MedicinesRecordStatus>
{
    public void Configure(EntityTypeBuilder<MedicinesRecordStatus> builder)
    {
        builder.ToTable("medicines_record_status", "ukps");
        builder.HasKey(x => new { x.MedicinesProductDetailId, x.MedicineStatusTypeId });

        builder.HasOne(x => x.MedicinesProductDetail)
               .WithMany(x => x.RecordStatuses)
               .HasForeignKey(x => x.MedicinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MedicineStatusType)
               .WithMany()
               .HasForeignKey(x => x.MedicineStatusTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesCompanyInfoConfiguration : IEntityTypeConfiguration<MedicinesCompanyInfo>
{
    public void Configure(EntityTypeBuilder<MedicinesCompanyInfo> builder)
    {
        builder.ToTable("medicines_company_info", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsOriginatorCompany).HasConversion<string>();
        builder.Property(x => x.IsCoMarketed).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_company_info_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesDetailConfiguration : IEntityTypeConfiguration<MedicinesDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesDetail> builder)
    {
        builder.ToTable("medicines_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsPersonalisedMedicine).HasConversion<string>();
        builder.Property(x => x.IsRepurposedMedicine).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesEamsPimConfiguration : IEntityTypeConfiguration<MedicinesEamsPim>
{
    public void Configure(EntityTypeBuilder<MedicinesEamsPim> builder)
    {
        builder.ToTable("medicines_eams_pim", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.PimDesignationStatus).HasConversion<string>();
        builder.Property(x => x.WillSubmitToEams).HasConversion<string>();
        builder.Property(x => x.EamsOpinionDecision).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_eams_pim_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EamsSubmissionDate)
               .WithMany()
               .HasForeignKey(x => x.EamsSubmissionDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EamsOpinionDate)
               .WithMany()
               .HasForeignKey(x => x.EamsOpinionDateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesEuStatusConfiguration : IEntityTypeConfiguration<MedicinesEuStatus>
{
    public void Configure(EntityTypeBuilder<MedicinesEuStatus> builder)
    {
        builder.ToTable("medicines_eu_status", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EuOrphanStatus).HasConversion<string>();
        builder.Property(x => x.EuAtmpClassificationStatus).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_eu_status_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EuOrphanGrantedDate)
               .WithMany()
               .HasForeignKey(x => x.EuOrphanGrantedDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AtmpClassification)
               .WithMany()
               .HasForeignKey(x => x.AtmpClassificationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesPatientIdentificationConfiguration
    : IEntityTypeConfiguration<MedicinesPatientIdentification>
{
    public void Configure(EntityTypeBuilder<MedicinesPatientIdentification> builder)
    {
        builder.ToTable("medicines_patient_identification", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ScreeningRequired).HasConversion<string>();
        builder.Property(x => x.UrgentIdentificationRequired).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_patient_identification_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesLaboratoryTestingConfiguration
    : IEntityTypeConfiguration<MedicinesLaboratoryTesting>
{
    public void Configure(EntityTypeBuilder<MedicinesLaboratoryTesting> builder)
    {
        builder.ToTable("medicines_laboratory_testing", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DiagnosticTestRequired).HasConversion<string>();
        builder.Property(x => x.GenomicTestRequired).HasConversion<string>();
        builder.Property(x => x.GenomicTestInNationalDirectory).HasConversion<string>();
        builder.Property(x => x.GenomicTurnaroundConsiderations).HasConversion<string>();
        builder.Property(x => x.GenomicTestMandatory).HasConversion<string>();
        builder.Property(x => x.MonitoringTestsRequired).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_laboratory_testing_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GenomicSampleType)
               .WithMany()
               .HasForeignKey(x => x.GenomicSampleTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PatientPathwayPoint)
               .WithMany()
               .HasForeignKey(x => x.PatientPathwayPointId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesTreatmentDetailConfiguration : IEntityTypeConfiguration<MedicinesTreatmentDetail>
{
    public void Configure(EntityTypeBuilder<MedicinesTreatmentDetail> builder)
    {
        builder.ToTable("medicines_treatment_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ProposedPlaceInTherapy).IsRequired();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_treatment_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesServiceImpactConfiguration : IEntityTypeConfiguration<MedicinesServiceImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesServiceImpact> builder)
    {
        builder.ToTable("medicines_service_impact", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ExistingNhsService).HasConversion<string>();
        builder.Property(x => x.CompassionateAccessAvailable).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_service_impact_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UkPatientPopulationRange)
               .WithMany()
               .HasForeignKey(x => x.UkPatientPopulationRangeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesBudgetImpactConfiguration : IEntityTypeConfiguration<MedicinesBudgetImpact>
{
    public void Configure(EntityTypeBuilder<MedicinesBudgetImpact> builder)
    {
        builder.ToTable("medicines_budget_impact", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IndicationSpecificPricingPlanned).HasConversion<string>();
        builder.Property(x => x.NetUkBudgetImpactOver5M).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_medicines_budget_impact_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicinesPasRegionConfiguration : IEntityTypeConfiguration<MedicinesPasRegion>
{
    public void Configure(EntityTypeBuilder<MedicinesPasRegion> builder)
    {
        builder.ToTable("medicines_pas_region", "ukps");
        builder.HasKey(x => new { x.MedicinesBudgetImpactId, x.PasRegionId });

        builder.HasOne(x => x.MedicinesBudgetImpact)
               .WithMany(x => x.PasRegions)
               .HasForeignKey(x => x.MedicinesBudgetImpactId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PasRegion)
               .WithMany()
               .HasForeignKey(x => x.PasRegionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
