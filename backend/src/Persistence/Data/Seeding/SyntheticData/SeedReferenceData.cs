using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using static UKPS.Api.Persistence.Data.Seeding.SyntheticData.SyntheticReferenceData;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// The medicines reference data entities, indexed by the codes and labels the synthetic records
/// use to refer to them.
/// </summary>
internal sealed class SeedReferenceData
{
    private static readonly HashSet<string> _medicinesOnlyProcedures = new(
        ["Access consortium", "Project Orbis"],
        StringComparer.Ordinal
    );

    private SeedReferenceData(SyntheticReferenceData data)
    {
        BnfChapters = CreateHierarchy(
            data.BnfChapters,
            entry => new BnfChapter
            {
                Code = entry.Code,
                Label = entry.Label,
                DisplayOrder = entry.DisplayOrder,
            },
            (child, parent) => child.Parent = parent
        );
        TherapeuticAreas = CreateHierarchy(
            data.TherapeuticAreas,
            entry => new TherapeuticArea { Label = entry.Label, DisplayOrder = entry.DisplayOrder },
            (child, parent) => child.Parent = parent
        );
        FormulationTypes = CreateList(
            data.FormulationTypes,
            e => new FormulationType { Label = e.Label }
        );
        MhraProcedureTypes = CreateList(
            data.MhraProcedureTypes,
            e => new MhraProcedureType
            {
                Label = e.Label,
                RelevantTo = _medicinesOnlyProcedures.Contains(e.Label)
                    ? ReferenceDataType.MedicinesOnly
                    : ReferenceDataType.Shared,
            }
        );
        IrpReferenceRegulators = CreateList(
            data.IrpReferenceRegulators,
            e => new IrpReferenceRegulator { Label = e.Label }
        );
        IrpRoutes = CreateList(data.IrpRoutes, e => new IrpRoute { Label = e.Label });
        AtmpClassifications = CreateList(
            data.AtmpClassifications,
            e => new AtmpClassification { Label = e.Label }
        );
        PatientPathwayPoints = CreateList(
            data.PatientPathwayPoints,
            e => new PatientPathwayPoint { Label = e.Label }
        );
        UkPatientPopulationRanges = CreateList(
            data.UkPatientPopulationRanges,
            e => new UkPatientPopulationRange { Label = e.Label, SortOrder = e.DisplayOrder }
        );
    }

    public IReadOnlyDictionary<string, BnfChapter> BnfChapters { get; }
    public IReadOnlyDictionary<string, TherapeuticArea> TherapeuticAreas { get; }
    public IReadOnlyDictionary<string, FormulationType> FormulationTypes { get; }
    public IReadOnlyDictionary<string, MhraProcedureType> MhraProcedureTypes { get; }
    public IReadOnlyDictionary<string, IrpReferenceRegulator> IrpReferenceRegulators { get; }
    public IReadOnlyDictionary<string, IrpRoute> IrpRoutes { get; }
    public IReadOnlyDictionary<string, AtmpClassification> AtmpClassifications { get; }
    public IReadOnlyDictionary<string, PatientPathwayPoint> PatientPathwayPoints { get; }
    public IReadOnlyDictionary<string, UkPatientPopulationRange> UkPatientPopulationRanges { get; }

    public static SeedReferenceData Create(SyntheticReferenceData data) => new(data);

    public IEnumerable<object> GetAllEntities() =>
        BnfChapters
            .Values.Cast<object>()
            .Concat(TherapeuticAreas.Values)
            .Concat(FormulationTypes.Values)
            .Concat(MhraProcedureTypes.Values)
            .Concat(IrpReferenceRegulators.Values)
            .Concat(IrpRoutes.Values)
            .Concat(AtmpClassifications.Values)
            .Concat(PatientPathwayPoints.Values)
            .Concat(UkPatientPopulationRanges.Values);

    /// <summary>Finds a reference data entry, failing loudly if the synthetic data refers to one that does not exist.</summary>
    public static T? Find<T>(IReadOnlyDictionary<string, T> entries, string? key)
        where T : class
    {
        if (key is null)
        {
            return null;
        }

        return entries.TryGetValue(key, out T? entry)
            ? entry
            : throw new InvalidOperationException(
                $"Synthetic data refers to unknown {typeof(T).Name} '{key}'."
            );
    }

    private static Dictionary<string, T> CreateHierarchy<T>(
        IEnumerable<SyntheticHierarchyEntry> entries,
        Func<SyntheticHierarchyEntry, T> create,
        Action<T, T> setParent
    )
    {
        Dictionary<string, T> byCode = new(StringComparer.Ordinal);
        foreach (SyntheticHierarchyEntry entry in entries)
        {
            T entity = create(entry);
            if (entry.ParentCode is not null)
            {
                setParent(entity, byCode[entry.ParentCode]);
            }
            byCode.Add(entry.Code, entity);
        }
        return byCode;
    }

    private static Dictionary<string, T> CreateList<T>(
        IEnumerable<SyntheticListEntry> entries,
        Func<SyntheticListEntry, T> create
    ) => entries.ToDictionary(entry => entry.Label, create, StringComparer.Ordinal);
}
