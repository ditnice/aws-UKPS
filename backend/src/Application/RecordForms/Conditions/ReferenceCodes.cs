using System.Globalization;

namespace UKPS.Api.Application.RecordForms.Conditions;

/// <summary>
/// The codes of the reference data that conditions name, by ID. Conditions name codes because
/// reference IDs are not a contract, so the codes are loaded with the answers on each request.
/// </summary>
internal sealed class ReferenceCodes
{
    private readonly Dictionary<Type, Dictionary<string, string>> _codesById;
    private readonly Dictionary<Type, Dictionary<string, string>> _idsByCode;

    public static ReferenceCodes None { get; } =
        new(new Dictionary<Type, IReadOnlyList<(int Id, string Code)>>());

    /// <summary>Indexes the rows both ways.</summary>
    /// <param name="rows">The ID and code of every row, by reference data entity.</param>
    public ReferenceCodes(IReadOnlyDictionary<Type, IReadOnlyList<(int Id, string Code)>> rows)
    {
        _codesById = rows.ToDictionary(
            static x => x.Key,
            static x =>
                x.Value.ToDictionary(
                    static r => r.Id.ToString(CultureInfo.InvariantCulture),
                    static r => r.Code,
                    StringComparer.Ordinal
                )
        );
        _idsByCode = rows.ToDictionary(
            static x => x.Key,
            static x =>
                x.Value.ToDictionary(
                    static r => r.Code,
                    static r => r.Id.ToString(CultureInfo.InvariantCulture),
                    StringComparer.Ordinal
                )
        );
    }

    /// <summary>The codes of the IDs; an ID with no loaded code is left out.</summary>
    public IEnumerable<string> CodesOf(Type referenceType, IEnumerable<string> ids) =>
        Translate(_codesById, referenceType, ids);

    /// <summary>The IDs of the codes; a code with no loaded row is left out.</summary>
    public IEnumerable<string> IdsOf(Type referenceType, IEnumerable<string> codes) =>
        Translate(_idsByCode, referenceType, codes);

    private static string[] Translate(
        Dictionary<Type, Dictionary<string, string>> map,
        Type referenceType,
        IEnumerable<string> values
    )
    {
        Dictionary<string, string> lookup = map.TryGetValue(referenceType, out var found)
            ? found
            : throw new InvalidOperationException(
                $"The codes of {referenceType.Name} were not loaded."
            );
        return values.Select(value => lookup.GetValueOrDefault(value)).OfType<string>().ToArray();
    }
}
