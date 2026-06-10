using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class VaccineDiseaseAreaConfiguration : ReferenceDataBaseConfiguration<VaccineDiseaseArea>
{
    protected override string TableName => "vaccine_disease_area";
}
