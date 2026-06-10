using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class MhraProcedureTypeConfiguration : ReferenceDataBaseConfiguration<MhraProcedureType>
{
    protected override string TableName => "mhra_procedure_type";

    public override void Configure(EntityTypeBuilder<MhraProcedureType> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.RelevantTo).HasConversion<string>();
    }
}
