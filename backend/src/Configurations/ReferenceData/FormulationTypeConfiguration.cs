using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class FormulationTypeConfiguration : ReferenceDataBaseConfiguration<FormulationType>
{
    protected override string TableName => "formulation_type";
}
