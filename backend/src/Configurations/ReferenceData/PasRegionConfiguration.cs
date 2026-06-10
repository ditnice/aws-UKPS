using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class PasRegionConfiguration : ReferenceDataBaseConfiguration<PasRegion>
{
    protected override string TableName => "pas_region";
}
