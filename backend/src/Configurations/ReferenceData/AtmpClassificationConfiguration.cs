using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class AtmpClassificationConfiguration : ReferenceDataBaseConfiguration<AtmpClassification>
{
    protected override string TableName => "atmp_classification";
}
