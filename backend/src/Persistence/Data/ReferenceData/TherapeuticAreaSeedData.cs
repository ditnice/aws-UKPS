namespace UKPS.Api.Persistence.Data.ReferenceData;

/// <summary>
/// The therapeutic areas, seeded by migration. Labels include their number, as shown to
/// users (e.g. "1: Addiction").
/// </summary>
internal static class TherapeuticAreaSeedData
{
    public static IReadOnlyList<(int Id, string Label)> Areas { get; } =
    [
        (1, "1: Addiction"),
        (2, "2: Allergy"),
        (3, "3: Analgesia, Anesthesiology, or Anti-inflammatory"),
        (4, "4: Cardiovascular Disease"),
        (5, "5: Dermatology"),
        (6, "6: Endocrinology, Metabolism, or Bone"),
        (7, "7: Gastroenterology"),
        (8, "8: Hematology/Coagulation"),
        (9, "9: Immunomodulators"),
        (10, "10: Viral infectious disease"),
        (11, "11: Non-viral infectious Disease"),
        (12, "12: Medical Imaging"),
        (13, "13: Neurology"),
        (14, "14: Oncology"),
        (15, "15: Ophthalmology"),
        (16, "16: Psychiatry"),
        (17, "17: Pulmonary"),
        (18, "18: Renal disease"),
        (19, "19: Rheumatology"),
        (20, "20: Urologic"),
    ];
}
