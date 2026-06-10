using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class UkPatientPopulationRangeConfiguration
    : ReferenceDataBaseConfiguration<UkPatientPopulationRange>
{
    protected override string TableName => "uk_patient_population_range";

    public override void Configure(EntityTypeBuilder<UkPatientPopulationRange> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
