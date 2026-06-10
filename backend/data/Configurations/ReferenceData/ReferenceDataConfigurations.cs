using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

/// <summary>
/// Shared base configuration for simple reference data tables
/// (id, label, is_archived).
/// </summary>
public abstract class ReferenceDataBaseConfiguration<T> : IEntityTypeConfiguration<T>
    where T : ReferenceDataBase
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName, "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Label).IsRequired();
    }
}

public class FormulationTypeConfiguration : ReferenceDataBaseConfiguration<FormulationType>
{
    protected override string TableName => "formulation_type";
}

public class MedicineTechnologyStatusConfiguration
    : ReferenceDataBaseConfiguration<MedicineTechnologyStatus>
{
    protected override string TableName => "medicine_technology_status";
}

public class MhraProcedureTypeConfiguration : ReferenceDataBaseConfiguration<MhraProcedureType>
{
    protected override string TableName => "mhra_procedure_type";

    public override void Configure(EntityTypeBuilder<MhraProcedureType> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.RelevantTo).HasConversion<string>();
    }
}

public class IrpReferenceRegulatorConfiguration
    : ReferenceDataBaseConfiguration<IrpReferenceRegulator>
{
    protected override string TableName => "irp_reference_regulator";
}

public class IrpRouteConfiguration : ReferenceDataBaseConfiguration<IrpRoute>
{
    protected override string TableName => "irp_route";
}

public class AtmpClassificationConfiguration : ReferenceDataBaseConfiguration<AtmpClassification>
{
    protected override string TableName => "atmp_classification";
}

public class GenomicSampleTypeConfiguration : ReferenceDataBaseConfiguration<GenomicSampleType>
{
    protected override string TableName => "genomic_sample_type";
}

public class PatientPathwayPointConfiguration : ReferenceDataBaseConfiguration<PatientPathwayPoint>
{
    protected override string TableName => "patient_pathway_point";
}

public class UkPatientPopulationRangeConfiguration
    : ReferenceDataBaseConfiguration<UkPatientPopulationRange>
{
    protected override string TableName => "uk_patient_population_range";

    public override void Configure(EntityTypeBuilder<UkPatientPopulationRange> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.SortOrder).IsRequired();
    }
}

public class PasRegionConfiguration : ReferenceDataBaseConfiguration<PasRegion>
{
    protected override string TableName => "pas_region";
}

public class VaccineAdministrationRouteConfiguration
    : ReferenceDataBaseConfiguration<VaccineAdministrationRoute>
{
    protected override string TableName => "vaccine_administration_route";
}

public class VaccineDiseaseAreaConfiguration : ReferenceDataBaseConfiguration<VaccineDiseaseArea>
{
    protected override string TableName => "vaccine_disease_area";
}

public class VaccineStorageRequirementConfiguration
    : ReferenceDataBaseConfiguration<VaccineStorageRequirement>
{
    protected override string TableName => "vaccine_storage_requirement";
}

public class VaccinePlatformConfiguration : ReferenceDataBaseConfiguration<VaccinePlatform>
{
    protected override string TableName => "vaccine_platform";
}

public class BnfChapterConfiguration : IEntityTypeConfiguration<BnfChapter>
{
    public void Configure(EntityTypeBuilder<BnfChapter> builder)
    {
        builder.ToTable("bnf_chapter", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Label).IsRequired();

        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TherapeuticAreaConfiguration : IEntityTypeConfiguration<TherapeuticArea>
{
    public void Configure(EntityTypeBuilder<TherapeuticArea> builder)
    {
        builder.ToTable("therapeutic_area", "ukps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Label).IsRequired();

        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
