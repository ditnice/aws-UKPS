using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Persistence.Configurations.ReferenceData;

internal sealed class PatientPathwayPointConfiguration
    : ReferenceDataBaseConfiguration<PatientPathwayPoint>
{
    protected override string TableName => "patient_pathway_point";
}
