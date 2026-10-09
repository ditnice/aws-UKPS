using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// The content form for one <see cref="Persistence.Enums.RecordType"/>. Built once at startup and
/// shared; it holds structure only (no per-request state).
/// </summary>
internal sealed class FormDefinition
{
    private readonly Dictionary<string, PageDefinition> _pagesById;
    private readonly Dictionary<string, int> _pageIndexes;

    public FormDefinition(
        RecordType recordType,
        string version,
        IReadOnlyList<SectionDefinition> sections
    )
    {
        RecordType = recordType;
        Version = version;
        Sections = sections;
        Pages = [.. sections.SelectMany(section => section.Pages)];
        // Duplicate IDs are reported by FormDefinitionValidator; keep the first here.
        _pagesById = new Dictionary<string, PageDefinition>(StringComparer.Ordinal);
        _pageIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < Pages.Count; index++)
        {
            if (_pagesById.TryAdd(Pages[index].Id, Pages[index]))
            {
                _pageIndexes[Pages[index].Id] = index;
            }
        }
    }

    public RecordType RecordType { get; }

    /// <summary>
    /// Echoed by clients on save; bump whenever the definition changes so stale clients are
    /// rejected.
    /// </summary>
    public string Version { get; }

    public IReadOnlyList<SectionDefinition> Sections { get; }

    /// <summary>Every page, in form order.</summary>
    public IReadOnlyList<PageDefinition> Pages { get; }

    public PageDefinition FirstPage => Pages[0];

    public PageDefinition? FindPage(string pageId) => _pagesById.GetValueOrDefault(pageId);

    /// <summary>The page after <paramref name="page"/> in form order, or <c>null</c> on the last page.</summary>
    public PageDefinition? NextPage(PageDefinition page) => PageAt(IndexOf(page) + 1);

    /// <summary>The page before <paramref name="page"/> in form order, or <c>null</c> on the first page.</summary>
    public PageDefinition? PreviousPage(PageDefinition page) => PageAt(IndexOf(page) - 1);

    private int IndexOf(PageDefinition page) =>
        _pageIndexes.TryGetValue(page.Id, out var index)
            ? index
            : throw new ArgumentException(
                $"Page '{page.Id}' is not part of this form.",
                nameof(page)
            );

    private PageDefinition? PageAt(int index) =>
        index >= 0 && index < Pages.Count ? Pages[index] : null;
}
