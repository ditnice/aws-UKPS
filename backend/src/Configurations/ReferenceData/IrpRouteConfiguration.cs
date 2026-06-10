using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class IrpRouteConfiguration : ReferenceDataBaseConfiguration<IrpRoute>
{
    protected override string TableName => "irp_route";
}
