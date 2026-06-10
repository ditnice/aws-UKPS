using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class IrpRouteConfiguration : ReferenceDataBaseConfiguration<IrpRoute>
{
    protected override string TableName => "irp_route";
}
