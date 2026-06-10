using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class PasRegionConfiguration : ReferenceDataBaseConfiguration<PasRegion>
{
    protected override string TableName => "pas_region";
}
