using UKPS.Data.Entities.ReferenceData;

namespace UKPS.Data.Configurations.ReferenceData;

public class PatientPathwayPointConfiguration : ReferenceDataBaseConfiguration<PatientPathwayPoint>
{
    protected override string TableName => "patient_pathway_point";
}
