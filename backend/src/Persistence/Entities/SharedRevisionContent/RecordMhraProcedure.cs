using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

[FormEntity]
internal sealed class RecordMhraProcedure
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormPage(MedicineFormLayout.UkMarketAuthorisation)]
    [FormField(
        "record_mhra_procedure.mhra_procedure_type_id",
        "Select the procedure you currently expect to use for UK authorisation",
        FormControl.Radio,
        Order = 10,
        Hint = "Select unknown at this stage if the route has not been confirmed."
    )]
    [FormRequired("Select the procedure you currently expect to use for UK authorisation")]
    public int? MhraProcedureTypeId { get; set; }

    /// <summary>
    /// FK to IrpReferenceRegulator. On the IRP page, which shows only when MhraProcedureType = IRP.
    /// </summary>
    [FormPage(MedicineFormLayout.InternationalRecognition)]
    [FormField(
        "record_mhra_procedure.irp_reference_regulator_id",
        "Select the reference regulator",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select a reference regulator")]
    public int? IrpReferenceRegulatorId { get; set; }

    [FormPage(MedicineFormLayout.UkMarketAuthorisation)]
    [FormField(
        "record_mhra_procedure.procedure_details",
        "MHRA regulatory procedure details (optional)",
        FormControl.Textarea,
        Order = 20,
        Hint = "Enter additional information that may be relevant to horizon scanners."
    )]
    [FormMaxLength(2000, "MHRA regulatory procedure details must be 2000 characters or fewer")]
    public string? ProcedureDetails { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.MhraProcedureType? MhraProcedureType { get; set; }
    public ReferenceData.IrpReferenceRegulator? IrpReferenceRegulator { get; set; }
}
