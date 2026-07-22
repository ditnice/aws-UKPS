using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class IrpReferenceRegulatorConfiguration
    : ReferenceDataBaseConfiguration<IrpReferenceRegulator>
{
    protected override string TableName => "irp_reference_regulator";
}
