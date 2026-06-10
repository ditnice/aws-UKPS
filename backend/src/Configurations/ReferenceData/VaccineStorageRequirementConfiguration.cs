using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class VaccineStorageRequirementConfiguration
    : ReferenceDataBaseConfiguration<VaccineStorageRequirement>
{
    protected override string TableName => "vaccine_storage_requirement";
}
