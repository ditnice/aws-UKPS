namespace UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

/// <summary>Junction table: routes of administration selected for a vaccine record.</summary>
internal sealed class VaccinesAdministrationRoute
{
    public int VaccinesTechnologyId { get; set; }
    public int AdministrationRouteId { get; set; }

    // Navigation
    public VaccinesTechnology? VaccinesTechnology { get; set; }
    public ReferenceData.VaccineAdministrationRoute? AdministrationRoute { get; set; }
}
