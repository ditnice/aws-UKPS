using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class AtmpClassificationConfiguration : ReferenceDataBaseConfiguration<AtmpClassification>
{
    protected override string TableName => "atmp_classification";
}
