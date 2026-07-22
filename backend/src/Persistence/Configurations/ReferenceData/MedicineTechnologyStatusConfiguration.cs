using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class MedicineTechnologyStatusConfiguration
    : ReferenceDataBaseConfiguration<MedicineTechnologyStatus>
{
    protected override string TableName => "medicine_technology_status";
}
