using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class MedicineTechnologyStatusConfiguration
    : ReferenceDataBaseConfiguration<MedicineTechnologyStatus>
{
    protected override string TableName => "medicine_technology_status";
}
