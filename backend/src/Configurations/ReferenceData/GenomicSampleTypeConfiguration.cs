using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

public class GenomicSampleTypeConfiguration : ReferenceDataBaseConfiguration<GenomicSampleType>
{
    protected override string TableName => "genomic_sample_type";
}
