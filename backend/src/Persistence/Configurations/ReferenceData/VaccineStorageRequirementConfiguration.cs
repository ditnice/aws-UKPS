using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class VaccineStorageRequirementConfiguration
    : ReferenceDataBaseConfiguration<VaccineStorageRequirement>
{
    protected override string TableName => "vaccine_storage_requirement";
}
