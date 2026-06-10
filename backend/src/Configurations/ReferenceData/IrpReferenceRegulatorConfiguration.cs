using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class IrpReferenceRegulatorConfiguration
    : ReferenceDataBaseConfiguration<IrpReferenceRegulator>
{
    protected override string TableName => "irp_reference_regulator";
}
