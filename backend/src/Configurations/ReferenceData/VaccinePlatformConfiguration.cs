using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class VaccinePlatformConfiguration : ReferenceDataBaseConfiguration<VaccinePlatform>
{
    protected override string TableName => "vaccine_platform";
}
