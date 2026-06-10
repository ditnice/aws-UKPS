using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class VaccinePlatformConfiguration : ReferenceDataBaseConfiguration<VaccinePlatform>
{
    protected override string TableName => "vaccine_platform";
}
