using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class VaccineAdministrationRouteConfiguration
    : ReferenceDataBaseConfiguration<VaccineAdministrationRoute>
{
    protected override string TableName => "vaccine_administration_route";
}
