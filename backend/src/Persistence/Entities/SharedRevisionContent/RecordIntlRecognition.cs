using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

internal sealed class RecordIntlRecognition
{
    public int Id { get; set; }
    public int RevisionId { get; set; }
    public int? IrpReferenceRegulatorId { get; set; }
    public int? IrpRouteId { get; set; }
    public YesNoUnknown? IntlConditionalApprovalAnticipated { get; set; }
    public int? IntlSubmissionDateId { get; set; }
    public int? IntlLicenceDateId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.IrpReferenceRegulator? IrpReferenceRegulator { get; set; }
    public ReferenceData.IrpRoute? IrpRoute { get; set; }
    public RegulatoryDate? IntlSubmissionDate { get; set; }
    public RegulatoryDate? IntlLicenceDate { get; set; }
}
