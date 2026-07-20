using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class UkPatientPopulationRangeConfiguration
    : ReferenceDataBaseConfiguration<UkPatientPopulationRange>
{
    protected override string TableName => "uk_patient_population_range";

    public override void Configure(EntityTypeBuilder<UkPatientPopulationRange> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
