using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class MedicineTechnologyStatusConfiguration
    : ReferenceDataBaseConfiguration<MedicineTechnologyStatus>
{
    protected override string TableName => "medicine_technology_status";
}
