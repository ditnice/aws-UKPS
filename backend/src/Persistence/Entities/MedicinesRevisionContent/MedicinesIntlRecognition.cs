using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>
/// Medicines only — International Recognition Procedure details.
/// Vaccines capture international regulatory activity as a simple gate plus free
/// text instead; see VaccinesRevisionContent.VaccinesIntlSubmission.
/// </summary>
[FormEntity]
[FormPageShowWhen(
    MedicineFormLayout.InternationalRecognition,
    "record_mhra_procedure.mhra_procedure_type_id",
    MhraProcedureTypeCodes.InternationalRecognition
)]
[FormPageShowWhen(
    MedicineFormLayout.InternationalSubmissionDate,
    "record_mhra_procedure.mhra_procedure_type_id",
    MhraProcedureTypeCodes.InternationalRecognition
)]
[FormPageShowWhen(
    MedicineFormLayout.InternationalLicenceDate,
    "record_mhra_procedure.mhra_procedure_type_id",
    MhraProcedureTypeCodes.InternationalRecognition
)]
[FormPageShowWhen(
    MedicineFormLayout.InternationalConditionalApproval,
    "record_mhra_procedure.mhra_procedure_type_id",
    MhraProcedureTypeCodes.InternationalRecognition
)]
internal sealed class MedicinesIntlRecognition
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormPage(MedicineFormLayout.InternationalRecognition)]
    [FormField(
        "medicines_intl_recognition.irp_route_id",
        "Select the intended route",
        FormControl.Radio,
        Order = 20
    )]
    [FormRequired("Select an option")]
    public int? IrpRouteId { get; set; }

    [FormPage(MedicineFormLayout.InternationalSubmissionDate)]
    [FormField(
        "medicines_intl_recognition.intl_submission_date",
        "What is the International Recognition Procedure submission date?",
        FormControl.PrecisionDate,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you plan to file this product with the reference regulator, or enter the actual date the dossier was formally accepted for review."
    )]
    [FormDateEvent(DateEventType.IntlSubmission, Name = "submission")]
    [FormRequired("Select an option")]
    public int? IntlSubmissionDateId { get; set; }

    [FormPage(MedicineFormLayout.InternationalLicenceDate)]
    [FormField(
        "medicines_intl_recognition.intl_licence_date",
        "What is the International Recognition Procedure regulatory licence date?",
        FormControl.PrecisionDate,
        Hint = "Choose the level of detail you are currently able to provide. Estimate the date you expect the reference authority will grant marketing authorisation for this indication, or enter the actual date the reference authority granted marketing authorisation for the specific indication in scope."
    )]
    [FormDateEvent(DateEventType.IntlLicence, Name = "licence")]
    [FormRequired("Select an option")]
    public int? IntlLicenceDateId { get; set; }

    [FormPage(MedicineFormLayout.InternationalConditionalApproval)]
    [FormField(
        "medicines_intl_recognition.intl_conditional_approval_anticipated",
        "Is international conditional approval anticipated for this product?",
        FormControl.Radio
    )]
    [FormRequired("Select an option")]
    [FormOptions(OptionKeys.YesNoUnknown)]
    public YesNoUnknown? IntlConditionalApprovalAnticipated { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.IrpRoute? IrpRoute { get; set; }
    public SharedRevisionContent.RegulatoryDate? IntlSubmissionDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? IntlLicenceDate { get; set; }
}
