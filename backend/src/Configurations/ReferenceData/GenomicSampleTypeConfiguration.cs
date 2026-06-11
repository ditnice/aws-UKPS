using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class GenomicSampleTypeConfiguration : ReferenceDataBaseConfiguration<GenomicSampleType>
{
    protected override string TableName => "genomic_sample_type";
}
