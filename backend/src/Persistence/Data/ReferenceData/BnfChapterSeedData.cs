namespace UKPS.Api.Persistence.Data.ReferenceData;

/// <summary>
/// The top-level British National Formulary chapters, seeded by migration. Sections
/// (e.g. 1.1) are not seeded yet; add them as children with <c>ParentId</c> set.
/// </summary>
internal static class BnfChapterSeedData
{
    public static IReadOnlyList<(int Id, string Code, string Label)> Chapters { get; } =
    [
        (1, "1", "Gastro-intestinal system"),
        (2, "2", "Cardiovascular system"),
        (3, "3", "Respiratory system"),
        (4, "4", "Central nervous system"),
        (5, "5", "Infections"),
        (6, "6", "Endocrine system"),
        (7, "7", "Obstetrics, gynaecology, and urinary-tract disorders"),
        (8, "8", "Malignant disease and immunosuppression"),
        (9, "9", "Nutrition and blood"),
        (10, "10", "Musculoskeletal and joint diseases"),
        (11, "11", "Eye"),
        (12, "12", "Ear, nose, and oropharynx"),
        (13, "13", "Skin"),
        (14, "14", "Immunological products and vaccines"),
        (15, "15", "Anaesthesia"),
    ];
}
