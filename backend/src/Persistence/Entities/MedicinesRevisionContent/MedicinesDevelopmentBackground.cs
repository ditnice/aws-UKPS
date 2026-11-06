using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(RecordType.Medicine, "development-background", "Development background", Order = 25)]
internal sealed class MedicinesDevelopmentBackground
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("repurposed-medicine", "Repurposed medicine", Order = 10)]
    [FormField(
        "medicines_development_background.is_repurposed_medicine",
        "Is this product a repurposed medicine?",
        FormControl.Radio,
        Order = 10
    )]
    [FormRequired("Select whether this product is a repurposed medicine")]
    public YesNoUnknown? IsRepurposedMedicine { get; set; }

    /// <summary>Differences from the current licensed indications.</summary>
    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("repurposed-medicine", "Repurposed medicine", Order = 10)]
    [FormField(
        "medicines_development_background.repurposed_medicine_details",
        "Enter the differences from the current licensed indications (optional)",
        FormControl.Textarea,
        Order = 20
    )]
    [FormShowWhen("medicines_development_background.is_repurposed_medicine", YesNoUnknown.Yes)]
    [FormMaxLength(2000, "Differences must be 2000 characters or fewer")]
    public string? RepurposedMedicineDetails { get; set; }

    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("originator", "Original developer", Order = 20)]
    [FormField(
        "medicines_development_background.is_originator_company",
        "Is your company the original developer of this product?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select no if you are a partner, licensee, or distributor."
    )]
    [FormRequired("Select whether your company is the original developer of this product")]
    public YesNoUnknown? IsOriginatorCompany { get; set; }

    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("originator", "Original developer", Order = 20)]
    [FormField(
        "medicines_development_background.originator_company_name",
        "Originator company name",
        FormControl.Text,
        Order = 20,
        Hint = "Enter the full name of the company who developed this product"
    )]
    [FormShowWhen("medicines_development_background.is_originator_company", YesNoUnknown.No)]
    [FormRequired("Enter the originator company name")]
    [FormMaxLength(200, "Originator company name must be 200 characters or fewer")]
    public string? OriginatorCompanyName { get; set; }

    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("co-marketing", "Co-marketing", Order = 30)]
    [FormField(
        "medicines_development_background.is_co_marketed",
        "Is this product co-marketed?",
        FormControl.Radio,
        Order = 10,
        Hint = "Select yes if another company will market the same product under the same or different brand name."
    )]
    [FormRequired("Select whether this product is co-marketed")]
    public YesNoUnknown? IsCoMarketed { get; set; }

    [FormTask(
        MedicineFormTasks.DevelopmentBackground,
        MedicineFormTasks.DevelopmentBackgroundTitle,
        Order = MedicineFormTasks.DevelopmentBackgroundOrder
    )]
    [FormPage("co-marketing", "Co-marketing", Order = 30)]
    [FormField(
        "medicines_development_background.co_marketing_company_name",
        "Co-marketing company name",
        FormControl.Text,
        Order = 20,
        Hint = "Enter the full name of the company who will co-market this product"
    )]
    [FormShowWhen("medicines_development_background.is_co_marketed", YesNoUnknown.Yes)]
    [FormRequired("Enter the co-marketing company name")]
    [FormMaxLength(200, "Co-marketing company name must be 200 characters or fewer")]
    public string? CoMarketingCompanyName { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
}
