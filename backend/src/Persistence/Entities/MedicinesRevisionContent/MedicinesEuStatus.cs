using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

/// <summary>Owns the EuOrphanGranted and AtmpClassificationRecommendation regulatory date rows.</summary>
[FormSection(
    RecordType.Medicine,
    "orphan-and-atmp",
    "Orphan medicine status and ATMP classification",
    Order = 55
)]
internal sealed class MedicinesEuStatus
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("orphan-status", "Orphan medicine status", Order = 10)]
    [FormField(
        "medicines_eu_status.eu_orphan_status",
        "Have you or will you apply for orphan medicine status in the EU?",
        FormControl.Radio,
        Order = 10,
        Hint = "Orphan designation applies to medicines intended for rare diseases and provides regulatory incentives in the EU."
    )]
    [FormOptions(OptionKeys.EuOrphanStatus)]
    [FormRequired(
        "Select yes, orphan status granted if your product has been granted an orphan medicine status in the EU"
    )]
    public DesignationStatus? EuOrphanStatus { get; set; }

    /// <summary>Conditional on EuOrphanStatus = Granted.</summary>
    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("orphan-status", "Orphan medicine status", Order = 10)]
    [FormField(
        "medicines_eu_status.eu_orphan_status_number",
        "EU orphan status number",
        FormControl.Text,
        Order = 30,
        Hint = "Provide the unique reference number assigned to the orphan designation by the EMA."
    )]
    [FormShowWhen("medicines_eu_status.eu_orphan_status", DesignationStatus.Granted)]
    [FormRequired("Enter the EU orphan status number")]
    public string? EuOrphanStatusNumber { get; set; }

    /// <summary>Conditional on EuOrphanStatus = Granted.</summary>
    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("orphan-status", "Orphan medicine status", Order = 10)]
    [FormField(
        "medicines_eu_status.eu_orphan_granted_date",
        "Date EU orphan medicine status granted",
        FormControl.ActualDate,
        Order = 20,
        Hint = "Enter the date the orphan designation was formally granted by the EMA."
    )]
    [FormDateEvent(DateEventType.EuOrphanGranted)]
    [FormShowWhen("medicines_eu_status.eu_orphan_status", DesignationStatus.Granted)]
    [FormRequired("Enter the date that EU orphan medicine status was granted")]
    public int? EuOrphanGrantedDateId { get; set; }

    /// <summary>EU ATMP classification status.</summary>
    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("atmp-classification", "ATMP classification", Order = 20)]
    [FormField(
        "medicines_eu_status.eu_atmp_classification_status",
        "Is this product classified as an ATMP in the EU?",
        FormControl.Radio,
        Order = 10,
        Hint = "ATMP classification applies to gene therapies, cell therapies, and tissue-engineered products."
    )]
    [FormOptions(OptionKeys.EuAtmpClassificationStatus)]
    [FormRequired("Select yes if this medicine is classified as an ATMP in the EU")]
    public DesignationStatus? EuAtmpClassificationStatus { get; set; }

    /// <summary>
    /// Date the ATMP classification recommendation was provided.
    /// Conditional on EuAtmpClassificationStatus = Granted.
    /// </summary>
    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("atmp-classification", "ATMP classification", Order = 20)]
    [FormField(
        "medicines_eu_status.atmp_recommendation_date",
        "Enter the date when the ATMP classification recommendation was provided",
        FormControl.ActualDate,
        Order = 20
    )]
    [FormDateEvent(DateEventType.AtmpClassificationRecommendation)]
    [FormShowWhen("medicines_eu_status.eu_atmp_classification_status", DesignationStatus.Granted)]
    [FormRequired("Enter the date the ATMP classification recommendation was provided")]
    public int? AtmpRecommendationDateId { get; set; }

    /// <summary>Conditional on EuAtmpClassificationStatus = Granted.</summary>
    [FormTask(
        MedicineFormTasks.OrphanAndAtmp,
        MedicineFormTasks.OrphanAndAtmpTitle,
        Order = MedicineFormTasks.OrphanAndAtmpOrder
    )]
    [FormPage("atmp-classification", "ATMP classification", Order = 20)]
    [FormField(
        "medicines_eu_status.atmp_classification_id",
        "Select the category that best reflects the classification outcome",
        FormControl.Radio,
        Order = 30
    )]
    [FormShowWhen("medicines_eu_status.eu_atmp_classification_status", DesignationStatus.Granted)]
    [FormRequired("Select the ATMP classification of your product")]
    public int? AtmpClassificationId { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public SharedRevisionContent.RegulatoryDate? EuOrphanGrantedDate { get; set; }
    public SharedRevisionContent.RegulatoryDate? AtmpRecommendationDate { get; set; }
    public ReferenceData.AtmpClassification? AtmpClassification { get; set; }
}
