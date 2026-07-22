using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class MhraProcedureTypeConfiguration
    : ReferenceDataBaseConfiguration<MhraProcedureType>
{
    protected override string TableName => "mhra_procedure_type";

    public override void Configure(EntityTypeBuilder<MhraProcedureType> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.RelevantTo);
    }
}
