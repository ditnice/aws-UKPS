using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class AtmpClassificationConfiguration
    : ReferenceDataBaseConfiguration<AtmpClassification>
{
    protected override string TableName => "atmp_classification";
}
