using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class FormulationTypeConfiguration : ReferenceDataBaseConfiguration<FormulationType>
{
    protected override string TableName => "formulation_type";
}
