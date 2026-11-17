using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class PatientPathwayPointConfiguration
    : ReferenceDataBaseConfiguration<PatientPathwayPoint>
{
    protected override string TableName => "patient_pathway_point";

    public override void Configure(EntityTypeBuilder<PatientPathwayPoint> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Code).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
