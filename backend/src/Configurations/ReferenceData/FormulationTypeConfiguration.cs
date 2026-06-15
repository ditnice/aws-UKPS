using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class FormulationTypeConfiguration : ReferenceDataBaseConfiguration<FormulationType>
{
    protected override string TableName => "formulation_type";
}
