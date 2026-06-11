using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class IrpRouteConfiguration : ReferenceDataBaseConfiguration<IrpRoute>
{
    protected override string TableName => "irp_route";
}
