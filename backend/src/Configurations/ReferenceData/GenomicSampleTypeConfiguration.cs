using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class GenomicSampleTypeConfiguration : ReferenceDataBaseConfiguration<GenomicSampleType>
{
    protected override string TableName => "genomic_sample_type";
}
