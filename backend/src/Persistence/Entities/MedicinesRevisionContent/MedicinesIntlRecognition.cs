using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Medicines only — International Recognition Procedure details.
/// Vaccines capture international regulatory activity as a simple gate plus free
/// text instead; see VaccinesRevisionContent.VaccinesIntlSubmission.
/// </summary>
internal sealed class MedicinesIntlRecognition
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
    public SharedRevisionContent.RegulatoryDate? IntlSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? IntlLicenceDate { get; set; }
}
