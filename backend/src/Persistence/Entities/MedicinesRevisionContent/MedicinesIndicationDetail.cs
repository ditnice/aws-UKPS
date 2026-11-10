using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(RecordType.Medicine, "indication-details", "Indication details", Order = 10)]
internal sealed class MedicinesIndicationDetail
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("indication", "Indication", Order = 10)]
    [FormField(
        "medicines_indication_detail.indication",
        "What is the indication this product is seeking a licence for?",
        FormControl.Textarea,
        Hint = "Describe the indication as you intend to apply for licensing in the UK. When you know them, include specific details such as line of therapy and subgroups.\n\nYou can edit these details at any time throughout product development."
    )]
    [FormRequired("Enter the indication")]
    [FormMaxLength(2000, "Indication must be 2000 characters or fewer")]
    public string? Indication { get; set; }

    /// <summary>
    /// The most specific BNF entry chosen: a subchapter when one is picked, otherwise the
    /// top-level chapter.
    /// </summary>
    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("bnf-chapter", "BNF chapter", Order = 20)]
    [FormField(
        "medicines_indication_detail.bnf_chapter_id",
        "Select the BNF chapter for this product",
        FormControl.HierarchicalSelect,
        Placeholder = "Choose BNF chapter",
        ChildLabel = "Select the BNF subchapter (optional)",
        ChildPlaceholder = "Choose BNF subchapter"
    )]
    [FormRequired("Choose a BNF chapter")]
    public int? BnfChapterId { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("paediatric", "Treating children", Order = 40)]
    [FormField(
        "medicines_indication_detail.indication_is_paediatric",
        "Is this product intended to treat children?",
        FormControl.Radio
    )]
    [FormRequired("Select whether this product is intended to treat children")]
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("cancer", "Treating cancer", Order = 50)]
    [FormField(
        "medicines_indication_detail.indication_is_cancer",
        "Is this a product intended to treat cancer?",
        FormControl.Radio
    )]
    [FormRequired("Select whether this product is intended to treat cancer")]
    public YesNoUnknown? IndicationIsCancer { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("rare-disease", "Treating a rare disease", Order = 60)]
    [FormField(
        "medicines_indication_detail.indication_is_rare_disease",
        "Is this a product intended to treat a rare disease?",
        FormControl.Radio,
        Hint = "A disease is considered rare if fewer than 5 in 10,000 people have it. Select yes if this medicine is intended to treat a rare disease, even if you have not applied for orphan status."
    )]
    [FormRequired("Select whether this product treats a rare disease")]
    public YesNoUnknown? IndicationIsRareDisease { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("formulation", "Formulation", Order = 70)]
    [FormField(
        "medicines_indication_detail.formulation_type_id",
        "What is the formulation?",
        FormControl.Select,
        Hint = "Select the formulation that best reflects the intended marketed product.",
        Placeholder = "Choose or search formulation"
    )]
    [FormRequired("Choose a formulation")]
    public int? FormulationTypeId { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("presentation", "Presentation", Order = 80)]
    [FormField(
        "medicines_indication_detail.presentation",
        "Describe the presentation of this product (optional)",
        FormControl.Textarea,
        Hint = "Provide the intended presentation if known. High-level or indicative information is acceptable at early stages."
    )]
    public string? Presentation { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("mode-of-action", "Mode of action", Order = 90)]
    [FormField(
        "medicines_indication_detail.mode_of_action",
        "What is the mode of action of this product? (optional)",
        FormControl.Textarea,
        Hint = "Briefly describe how the product works at a biological or pharmacological level."
    )]
    public string? ModeOfAction { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("dose-regimen", "Proposed dose regimen", Order = 100)]
    [FormField(
        "medicines_indication_detail.proposed_dose_regimen",
        "What is the proposed dose regimen for this product? (optional)",
        FormControl.Textarea,
        Hint = "Enter the intended dosing schedule. You can include the dose amount, dose frequency, route and duration.\n\nIf in early stages of development, you can enter high-level or indicative information and update this later."
    )]
    public string? ProposedDoseRegimen { get; set; }

    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("personalised-medicine", "Personalised medicine", Order = 110)]
    [FormField(
        "medicines_indication_detail.is_personalised_medicine",
        "Is this product a personalised medicine?",
        FormControl.Radio,
        Hint = "Personalised medicines are treatments chosen or adjusted to suit a person's genetic, biomarker, molecular or clinical characteristics. This can improve how well the treatment works and reduce side effects."
    )]
    [FormRequired("Select yes if the product is a personalised medicine")]
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }

    /// <summary>Multi-select technology status types for this record.</summary>
    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("technology-status", "Technology status", Order = 120)]
    [FormField(
        "medicines_indication_detail.medicine_technology_status",
        "What is the technology status for this product?",
        FormControl.Checkbox,
        Hint = "Select the classification that best describes this product."
    )]
    [FormRequired(
        "Select the technology status of this product. You can select multiple statuses."
    )]
    public MedicineTechnologyStatus? MedicineTechnologyStatus { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }

    /// <summary>
    /// Multi-select: up to 3 therapeutic areas. Each is the most specific area chosen: a disease
    /// or condition when one is picked, otherwise the top-level area.
    /// See MedicinesIndicationDetailTherapeuticArea junction table.
    /// </summary>
    [FormTask(
        MedicineFormTasks.IndicationDetails,
        MedicineFormTasks.IndicationDetailsTitle,
        Order = MedicineFormTasks.IndicationDetailsOrder
    )]
    [FormPage("therapeutic-area", "Therapeutic area", Order = 30)]
    [FormField(
        "medicines_indication_detail_therapeutic_area",
        "Select the most appropriate therapeutic area(s) for this product (optional)",
        FormControl.HierarchicalSelect,
        Display = "combobox",
        Hint = "You can select up to 3 therapeutic areas if this product is cross-cutting. If this product genuinely spans more than 3 therapeutic areas, select the 3 most clinically relevant. If you are unsure, you can move on and answer this question later.",
        Placeholder = "Choose a therapeutic area",
        ChildLabel = "Select the disease or condition (optional)",
        ChildPlaceholder = "Choose a disease or condition",
        AddAnotherLabel = "Add another therapy area"
    )]
    [FormMaxItems(3, "Select up to 3 therapeutic areas")]
    public ICollection<MedicinesIndicationDetailTherapeuticArea> TherapeuticAreas { get; set; } =
    [];
}
