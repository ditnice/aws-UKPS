using UKPS.Api.Entities.ReferenceData;

namespace UKPS.Api.Configurations.ReferenceData;

internal sealed class PatientPathwayPointConfiguration : ReferenceDataBaseConfiguration<PatientPathwayPoint>
{
    protected override string TableName => "patient_pathway_point";
}
