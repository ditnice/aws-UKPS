using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class IrpReferenceRegulatorConfiguration
    : ReferenceDataBaseConfiguration<IrpReferenceRegulator>
{
    protected override string TableName => "irp_reference_regulator";
}
