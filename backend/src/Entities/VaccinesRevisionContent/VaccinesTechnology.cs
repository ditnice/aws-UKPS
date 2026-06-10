using UKPS.Data.Enums;

namespace UKPS.Data.Entities.VaccinesRevisionContent;

public class VaccinesTechnology
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int VaccinePlatformId { get; set; }

    /// <summary>Free text; conditional on VaccinePlatform = Other.</summary>
    public string? VaccinePlatformOther { get; set; }

    public int AdministrationRouteId { get; set; }
    public YesNoNotYetConfirmed HasAdjuvant { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public ReferenceData.VaccinePlatform VaccinePlatform { get; set; } = null!;
    public ReferenceData.VaccineAdministrationRoute AdministrationRoute { get; set; } = null!;
    public ICollection<VaccinesAntigen> Antigens { get; set; } = [];
    public ICollection<VaccinesAdjuvant> Adjuvants { get; set; } = [];
}
