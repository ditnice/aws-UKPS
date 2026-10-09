namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

internal sealed record SyntheticReferenceData
{
    public required IReadOnlyList<SyntheticHierarchyEntry> BnfChapters { get; init; }
    public required IReadOnlyList<SyntheticHierarchyEntry> TherapeuticAreas { get; init; }
    public required IReadOnlyList<SyntheticListEntry> FormulationTypes { get; init; }
    public required IReadOnlyList<SyntheticListEntry> MhraProcedureTypes { get; init; }
    public required IReadOnlyList<SyntheticListEntry> IrpReferenceRegulators { get; init; }
    public required IReadOnlyList<SyntheticListEntry> IrpRoutes { get; init; }
    public required IReadOnlyList<SyntheticListEntry> AtmpClassifications { get; init; }
    public required IReadOnlyList<SyntheticListEntry> PatientPathwayPoints { get; init; }
    public required IReadOnlyList<SyntheticListEntry> UkPatientPopulationRanges { get; init; }

    internal sealed record SyntheticHierarchyEntry
    {
        public required string Code { get; init; }
        public required string Label { get; init; }
        public string? ParentCode { get; init; }
        public required int DisplayOrder { get; init; }
    }

    internal sealed record SyntheticListEntry
    {
        public required string Label { get; init; }
        public required int DisplayOrder { get; init; }
    }
}
