using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class PasRegionConfiguration : ReferenceDataBaseConfiguration<PasRegion>
{
    protected override string TableName => "pas_region";
}
