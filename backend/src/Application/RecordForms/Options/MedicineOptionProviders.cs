using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>The option providers used by the Medicine form.</summary>
internal static class MedicineOptionProviders
{
    public static void Register(OptionProviderRegistry registry)
    {
        registry.AddEnum(
            OptionKeys.YesNoUnknown,
            (YesNoUnknown.Yes, "Yes"),
            (YesNoUnknown.No, "No"),
            (YesNoUnknown.Unknown, "Unknown at this stage")
        );

        registry.AddEnum(
            OptionKeys.PaediatricStatus,
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
            (IndicationPaediatricStatus.Unknown, "Unknown at this stage")
        );

        registry.AddReferenceTree<BnfChapter>(
            OptionKeys.BnfChapters,
            q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id),
            x => new ReferenceOptionRow(x.Id, x.Code + ": " + x.Label, x.IsArchived, x.ParentId)
        );

        registry.AddReferenceTree<TherapeuticArea>(
            OptionKeys.TherapeuticAreas,
            q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id),
            x => new ReferenceOptionRow(x.Id, x.Code + ": " + x.Label, x.IsArchived, x.ParentId)
        );
    }
}
