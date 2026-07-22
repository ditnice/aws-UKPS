using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class VaccinePlatformConfiguration : ReferenceDataBaseConfiguration<VaccinePlatform>
{
    protected override string TableName => "vaccine_platform";
}
