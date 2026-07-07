namespace UKPS.Api.Entities.VaccinesRevisionContent;

internal sealed class VaccinesDiseaseDetail
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? DiseaseAreaId { get; set; }

    /// <summary>
    /// Free text. The disease or diseases this vaccine prevents.
    /// e.g. Measles, mumps and rubella. Single field covers combination vaccines.
    /// </summary>
    public required string DiseaseTarget { get; set; }

    /// <summary>
    /// Intended age group e.g. Infants under 12 months, Adults aged 65 and over,
    /// All ages, Unknown at this stage.
    /// </summary>
    public required string AgeGroup { get; set; }

    /// <summary>
    /// Intended risk group if applicable e.g. Immunocompromised individuals,
    /// Pregnant women. Null if targeting general population.
    /// </summary>
    public string? RiskGroup { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.VaccineDiseaseArea? DiseaseArea { get; set; }
    public ICollection<VaccinesPathogen> Pathogens { get; set; } = [];
}
