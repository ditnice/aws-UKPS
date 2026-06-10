using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class VaccineDiseaseAreaConfiguration : ReferenceDataBaseConfiguration<VaccineDiseaseArea>
{
    protected override string TableName => "vaccine_disease_area";
}
