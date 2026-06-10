using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class VaccineAdministrationRouteConfiguration
    : ReferenceDataBaseConfiguration<VaccineAdministrationRoute>
{
    protected override string TableName => "vaccine_administration_route";
}
