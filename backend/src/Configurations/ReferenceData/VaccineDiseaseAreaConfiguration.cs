using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class VaccineDiseaseAreaConfiguration
    : ReferenceDataBaseConfiguration<VaccineDiseaseArea>
{
    protected override string TableName => "vaccine_disease_area";
}
