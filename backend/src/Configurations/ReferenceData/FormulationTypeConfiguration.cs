using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class FormulationTypeConfiguration : ReferenceDataBaseConfiguration<FormulationType>
{
    protected override string TableName => "formulation_type";
}
