using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class VaccineAdministrationRouteConfiguration
    : ReferenceDataBaseConfiguration<VaccineAdministrationRoute>
{
    protected override string TableName => "vaccine_administration_route";
}
