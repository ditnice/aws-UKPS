using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class PasRegionConfiguration : ReferenceDataBaseConfiguration<PasRegion>
{
    protected override string TableName => "pas_region";
}
