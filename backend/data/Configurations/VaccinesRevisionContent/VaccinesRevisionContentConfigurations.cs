using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.VaccinesRevisionContent;

namespace UKPS.Data.Configurations.VaccinesRevisionContent;

public class VaccinesProductDetailConfiguration : IEntityTypeConfiguration<VaccinesProductDetail>
{
    public void Configure(EntityTypeBuilder<VaccinesProductDetail> builder)
    {
        builder.ToTable("vaccines_product_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RecordTitle).IsRequired();
        builder.Property(x => x.CompanyCode).IsRequired();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_product_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesCompanyCodeConfiguration : IEntityTypeConfiguration<VaccinesCompanyCode>
{
    public void Configure(EntityTypeBuilder<VaccinesCompanyCode> builder)
    {
        builder.ToTable("vaccines_company_code", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).IsRequired();

        builder.HasIndex(x => x.VaccinesProductDetailId)
               .HasDatabaseName("ix_vaccines_company_code_product_detail_id");

        builder.HasOne(x => x.VaccinesProductDetail)
               .WithMany(x => x.CompanyCodes)
               .HasForeignKey(x => x.VaccinesProductDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesCompanyInfoConfiguration : IEntityTypeConfiguration<VaccinesCompanyInfo>
{
    public void Configure(EntityTypeBuilder<VaccinesCompanyInfo> builder)
    {
        builder.ToTable("vaccines_company_info", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.IsOriginatorCompany).HasConversion<string>();
        builder.Property(x => x.HasBeenAcquired).HasConversion<string>();
        builder.Property(x => x.HasGrantFunding).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_company_info_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesDiseaseDetailConfiguration : IEntityTypeConfiguration<VaccinesDiseaseDetail>
{
    public void Configure(EntityTypeBuilder<VaccinesDiseaseDetail> builder)
    {
        builder.ToTable("vaccines_disease_detail", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.DiseaseTarget).IsRequired();
        builder.Property(x => x.AgeGroup).IsRequired();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_disease_detail_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DiseaseArea)
               .WithMany()
               .HasForeignKey(x => x.DiseaseAreaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesPathogenConfiguration : IEntityTypeConfiguration<VaccinesPathogen>
{
    public void Configure(EntityTypeBuilder<VaccinesPathogen> builder)
    {
        builder.ToTable("vaccines_pathogen", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.PathogenName).IsRequired();

        builder.HasIndex(x => x.VaccinesDiseaseDetailId)
               .HasDatabaseName("ix_vaccines_pathogen_disease_detail_id");

        builder.HasOne(x => x.VaccinesDiseaseDetail)
               .WithMany(x => x.Pathogens)
               .HasForeignKey(x => x.VaccinesDiseaseDetailId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesTechnologyConfiguration : IEntityTypeConfiguration<VaccinesTechnology>
{
    public void Configure(EntityTypeBuilder<VaccinesTechnology> builder)
    {
        builder.ToTable("vaccines_technology", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.HasAdjuvant).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_technology_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VaccinePlatform)
               .WithMany()
               .HasForeignKey(x => x.VaccinePlatformId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AdministrationRoute)
               .WithMany()
               .HasForeignKey(x => x.AdministrationRouteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesAntigenConfiguration : IEntityTypeConfiguration<VaccinesAntigen>
{
    public void Configure(EntityTypeBuilder<VaccinesAntigen> builder)
    {
        builder.ToTable("vaccines_antigen", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.AntigenName).IsRequired();

        builder.HasIndex(x => x.VaccinesTechnologyId)
               .HasDatabaseName("ix_vaccines_antigen_technology_id");

        builder.HasOne(x => x.VaccinesTechnology)
               .WithMany(x => x.Antigens)
               .HasForeignKey(x => x.VaccinesTechnologyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesAdjuvantConfiguration : IEntityTypeConfiguration<VaccinesAdjuvant>
{
    public void Configure(EntityTypeBuilder<VaccinesAdjuvant> builder)
    {
        builder.ToTable("vaccines_adjuvant", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.AdjuvantName).IsRequired();

        builder.HasIndex(x => x.VaccinesTechnologyId)
               .HasDatabaseName("ix_vaccines_adjuvant_technology_id");

        builder.HasOne(x => x.VaccinesTechnology)
               .WithMany(x => x.Adjuvants)
               .HasForeignKey(x => x.VaccinesTechnologyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesServiceReadinessConfiguration : IEntityTypeConfiguration<VaccinesServiceReadiness>
{
    public void Configure(EntityTypeBuilder<VaccinesServiceReadiness> builder)
    {
        builder.ToTable("vaccines_service_readiness", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.RequiresReconstitution).HasConversion<string>();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_service_readiness_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StorageRequirement)
               .WithMany()
               .HasForeignKey(x => x.StorageRequirementId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VaccinesPopulationConfiguration : IEntityTypeConfiguration<VaccinesPopulation>
{
    public void Configure(EntityTypeBuilder<VaccinesPopulation> builder)
    {
        builder.ToTable("vaccines_population", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasIndex(x => x.RevisionId).IsUnique()
               .HasDatabaseName("ix_vaccines_population_revision_id");

        builder.HasOne(x => x.Revision)
               .WithMany()
               .HasForeignKey(x => x.RevisionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
