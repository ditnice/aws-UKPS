using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(RecordType.Medicine, "indication-details", "Indication details", Order = 20)]
internal sealed class MedicinesIndicationDetail
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormPage("indication", "Indication", Order = 10)]
    [FormField(
        "medicines_indication_detail.indication",
        "What is the indication this product is seeking a licence for?",
        Control = FormControl.Textarea,
        Hint = "Describe the indication as you intend to apply for licensing in the UK. When you know them, include specific details such as line of therapy and subgroups.\n\nYou can edit these details at any time throughout product development."
    )]
    [FormRequired("Enter the indication this product is seeking a licence for")]
    [FormMaxLength(2000, "Indication must be 2000 characters or fewer")]
    public string? Indication { get; set; }

    /// <summary>
    /// The most specific BNF entry chosen: a subchapter when one is picked, otherwise the
    /// top-level chapter.
    /// </summary>
    [FormPage("bnf-chapter", "BNF chapter", Order = 20)]
    [FormField(
        "medicines_indication_detail.bnf_chapter_id",
        "Select the BNF chapter for this product",
        Control = FormControl.HierarchicalSelect,
        Placeholder = "Choose BNF chapter",
        ChildLabel = "Select the BNF subchapter (optional)",
        ChildPlaceholder = "Choose BNF subchapter"
    )]
    [FormRequired("Choose a BNF chapter")]
    public int? BnfChapterId { get; set; }

    [FormPage("paediatric", "Treating children", Order = 40)]
    [FormField(
        "medicines_indication_detail.indication_is_paediatric",
        "Is this product intended to treat children?"
    )]
    [FormRequired("Select whether this product is intended to treat children")]
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }

    [FormPage("cancer", "Treating cancer", Order = 50)]
    [FormField(
        "medicines_indication_detail.indication_is_cancer",
        "Is this a product intended to treat cancer?"
    )]
    [FormRequired("Select whether this product is intended to treat cancer")]
    public YesNoUnknown? IndicationIsCancer { get; set; }

    [FormPage("rare-disease", "Treating a rare disease", Order = 60)]
    [FormField(
        "medicines_indication_detail.indication_is_rare_disease",
        "Is this product intended to treat a rare disease?",
        Hint = "A disease is rare if fewer than 5 in 10,000 people have it."
    )]
    [FormRequired("Select whether this product treats a rare disease")]
    public YesNoUnknown? IndicationIsRareDisease { get; set; }

    [FormIgnore("No page designed yet")]
    public int? FormulationTypeId { get; set; }

    [FormIgnore("No page designed yet")]
    public string? Presentation { get; set; }

    [FormIgnore("No page designed yet")]
    public string? ModeOfAction { get; set; }

    [FormIgnore("No page designed yet")]
    public string? ProposedDoseRegimen { get; set; }

    [FormIgnore("No page designed yet")]
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }

    /// <summary>Multi-select technology status types for this record.</summary>
    [FormIgnore("No page designed yet")]
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
    [FormPage("therapeutic-area", "Therapeutic area", Order = 30)]
    [FormField(
        "medicines_indication_detail_therapeutic_area",
        "Select the most appropriate therapeutic area for this product (optional)",
        Control = FormControl.HierarchicalSelect,
        Display = "combobox",
        Hint = "Select up to 3 therapeutic areas. If you are unsure, you can move on and answer this question later.",
        Placeholder = "Choose a therapeutic area",
        ChildLabel = "Select the disease or condition (optional)",
        ChildPlaceholder = "Choose a disease or condition",
        AddAnotherLabel = "Add another therapy area"
    )]
    [FormMaxItems(3, "Select up to 3 therapeutic areas")]
    public ICollection<MedicinesIndicationDetailTherapeuticArea> TherapeuticAreas { get; set; } =
    [];
}
