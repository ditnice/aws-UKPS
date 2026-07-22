using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class AtmpClassificationConfiguration
    : ReferenceDataBaseConfiguration<AtmpClassification>
{
    protected override string TableName => "atmp_classification";
}
