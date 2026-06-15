using UKPS.Api.Enums;

namespace UKPS.Api.Entities.SharedRevisionContent;

internal sealed class RecordMhraDate
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? UkSubmissionDateId { get; set; }
    public int? UkLicenceDateId { get; set; }
    public int? UkLaunchDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision Revision { get; set; } = null!;
    public RegulatoryDate? UkSubmissionDate { get; set; }
    public RegulatoryDate? UkLicenceDate { get; set; }
    public RegulatoryDate? UkLaunchDate { get; set; }
}
