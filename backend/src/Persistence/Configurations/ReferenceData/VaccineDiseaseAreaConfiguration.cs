using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class VaccineDiseaseAreaConfiguration
    : ReferenceDataBaseConfiguration<VaccineDiseaseArea>
{
    protected override string TableName => "vaccine_disease_area";
}
