using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

internal sealed class VaccinesTechnology
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int VaccinePlatformId { get; set; }

    /// <summary>Free text; conditional on VaccinePlatform = Other.</summary>
    public string? VaccinePlatformOther { get; set; }

    public int AdministrationRouteId { get; set; }
    public YesNoNotYetConfirmed HasAdjuvant { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.VaccinePlatform? VaccinePlatform { get; set; }
    public ReferenceData.VaccineAdministrationRoute? AdministrationRoute { get; set; }
    public ICollection<VaccinesAntigen> Antigens { get; set; } = [];
    public ICollection<VaccinesAdjuvant> Adjuvants { get; set; } = [];
}
