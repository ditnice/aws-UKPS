using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class GenomicSampleTypeConfiguration
    : ReferenceDataBaseConfiguration<GenomicSampleType>
{
    protected override string TableName => "genomic_sample_type";
}
