using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>The option providers used by the Medicine form.</summary>
internal static class MedicineOptionProviders
{
    // Keep each method under 60 lines (MA0051): one method per form section.
    public static void Register(OptionProviderRegistry registry)
    {
        RegisterIndicationDetailsProviders(registry);
        RegisterLaboratoryTestingProviders(registry);
    }

    private static void RegisterIndicationDetailsProviders(OptionProviderRegistry registry)
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

    private static void RegisterLaboratoryTestingProviders(OptionProviderRegistry registry)
    {
        registry.AddEnum(
            OptionKeys.BiomarkerType,
            (BiomarkerType.GenomicBiomarker, "Genomic biomarker"),
            (BiomarkerType.NonGenomicBiomarker, "Non-genomic biomarker"),
            (BiomarkerType.Unknown, "Unknown at this stage")
        );

        registry.AddEnum(
            OptionKeys.GenomicTestNgtdRelationship,
            (GenomicTestNgtdRelationship.NewTest, "New test, target not currently in the NGTD"),
            (
                GenomicTestNgtdRelationship.ExistingTestNewIndication,
                "Existing test, new or expanded indication"
            ),
            (
                GenomicTestNgtdRelationship.ExistingTestSameIndication,
                "Existing test, same indication"
            ),
            (GenomicTestNgtdRelationship.Unknown, "Unknown at this stage")
        );

        registry.AddEnum(
            OptionKeys.GenomicTestMandatoryStatus,
            (
                GenomicTestMandatoryStatus.MandatoryNoAlternatives,
                "Testing is mandatory, and treatment cannot proceed without a positive result from this test"
            ),
            (
                GenomicTestMandatoryStatus.MandatoryAlternativesMayExist,
                "Testing is mandatory, but there may be suitable alternative tests"
            ),
            (
                GenomicTestMandatoryStatus.RecommendedNotRequired,
                "Testing is recommended but not required"
            ),
            (GenomicTestMandatoryStatus.Unknown, "Unknown at this stage")
        );

        registry.AddReference<PatientPathwayPoint>(
            OptionKeys.PatientPathwayPoints,
            q => q.OrderBy(x => x.Id),
            x => new ReferenceOptionRow(x.Id, x.Label, x.IsArchived, null)
        );
    }
}
