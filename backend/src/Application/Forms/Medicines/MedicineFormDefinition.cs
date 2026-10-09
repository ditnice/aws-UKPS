using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Options;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Forms.Medicines;

/// <summary>The content form for medicine records.</summary>
internal static class MedicineFormDefinition
{
    /// <summary>
    /// Bump whenever the definition changes; the fingerprint test enforces this.
    /// </summary>
    public const string Version = "2026.10.1";

    private const string UnknownAtThisStage = "Unknown at this stage";

    public static FormDefinition Create() =>
        Form.For(RecordType.Medicine, Version)
            .Section(
                "indication-details",
                "Indication details",
                s =>
                    s.Page("indication", "Indication", IndicationPage)
                        .Page("bnf-chapter", "BNF chapter", BnfChapterPage)
                        .Page("therapeutic-area", "Therapeutic area", TherapeuticAreaPage)
                        .Page("paediatric", "Treating children", PaediatricPage)
                        .Page("cancer", "Treating cancer", CancerPage)
            )
            .Build();

    private static void IndicationPage(PageBuilder p) =>
        p.Textarea(
            "medicines_product_detail.indication",
            "What is the indication this product is seeking a licence for?",
            Bind.Column((MedicinesProductDetail x) => x.Indication),
            q =>
                q.Hint(
                        "Describe the indication as you intend to apply for licensing in the UK. When you know them, include specific details such as line of therapy and subgroups.\n\nYou can edit these details at any time throughout product development."
                    )
                    .Required("Enter the indication this product is seeking a licence for")
                    .MaxLength(2000, "Indication must be 2000 characters or fewer")
        );

    private static void BnfChapterPage(PageBuilder p) =>
        p.Select(
            "medicines_product_detail.bnf_chapter_id",
            "Select the BNF chapter for this product",
            Bind.Column((MedicinesProductDetail x) => x.BnfChapterId),
            // Top-level chapters only.
            Choices.Reference<BnfChapter>(
                q =>
                    q.Where(x => x.ParentId == null)
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Code),
                x => new ReferenceOptionRow(x.Id, x.Code + ": " + x.Label, x.IsArchived)
            ),
            q => q.Required("Select a BNF chapter")
        );

    private static void TherapeuticAreaPage(PageBuilder p) =>
        p.Checkbox(
            "medicines_product_detail_therapeutic_area",
            "Select the most appropriate therapeutic area for this product (optional)",
            Bind.Junction<MedicinesProductDetail, MedicinesProductDetailTherapeuticArea>(
                x => x.MedicinesProductDetailId,
                x => x.TherapeuticAreaId
            ),
            // Leaves only, labelled with their parent if they have one. The table is flat
            // today, so this is every area, labelled as stored.
            Choices.Reference<TherapeuticArea>(
                q =>
                    q.Where(x => !x.Children.Any())
                        .OrderBy(x => x.Parent!.DisplayOrder)
                        .ThenBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Label),
                x => new ReferenceOptionRow(
                    x.Id,
                    x.Parent == null ? x.Label : x.Parent.Label + " › " + x.Label,
                    x.IsArchived
                )
            ),
            q =>
                q.Hint(
                        "Select up to 3 therapeutic areas. If you are unsure, you can move on and answer this question later."
                    )
                    .Display("combobox")
                    .MaxItems(3, "Select up to 3 therapeutic areas")
        );

    private static void PaediatricPage(PageBuilder p) =>
        p.Radio(
            "medicines_product_detail.indication_is_paediatric",
            "Is this product intended to treat children?",
            Bind.Column((MedicinesProductDetail x) => x.IndicationIsPaediatric),
            Choices.Enum(
                (
                    IndicationPaediatricStatus.ExclusivelyChildren,
                    "This product is exclusively for children"
                ),
                (
                    IndicationPaediatricStatus.BothChildrenAndAdults,
                    "This product is applicable for both children and adults"
                ),
                (
                    IndicationPaediatricStatus.ExclusivelyAdults,
                    "This product is exclusively for adults"
                ),
                (IndicationPaediatricStatus.Unknown, UnknownAtThisStage)
            ),
            q => q.Required("Select whether this product is intended to treat children")
        );

    private static void CancerPage(PageBuilder p) =>
        p.Radio(
            "medicines_product_detail.indication_is_cancer",
            "Is this a product intended to treat cancer?",
            Bind.Column((MedicinesProductDetail x) => x.IndicationIsCancer),
            Choices.Enum(
                (YesNoUnknown.Yes, "Yes"),
                (YesNoUnknown.No, "No"),
                (YesNoUnknown.Unknown, UnknownAtThisStage)
            ),
            q => q.Required("Select whether this product is intended to treat cancer")
        );
}
