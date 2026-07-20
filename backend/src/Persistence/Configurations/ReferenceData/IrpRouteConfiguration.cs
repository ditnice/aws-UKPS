using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class IrpRouteConfiguration : ReferenceDataBaseConfiguration<IrpRoute>
{
    protected override string TableName => "irp_route";
}
