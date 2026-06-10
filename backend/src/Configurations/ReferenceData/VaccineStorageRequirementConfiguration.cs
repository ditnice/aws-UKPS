using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class VaccineStorageRequirementConfiguration
    : ReferenceDataBaseConfiguration<VaccineStorageRequirement>
{
    protected override string TableName => "vaccine_storage_requirement";
}
