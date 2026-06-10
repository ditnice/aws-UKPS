using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class MedicineTechnologyStatusConfiguration
    : ReferenceDataBaseConfiguration<MedicineTechnologyStatus>
{
    protected override string TableName => "medicine_technology_status";
}
