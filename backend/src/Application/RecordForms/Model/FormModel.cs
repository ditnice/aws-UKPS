using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>The content form of one record type: ordered sections, each with ordered pages.</summary>
internal sealed class FormModel
{
    private readonly Dictionary<string, int> _pageIndex;

    public RecordType RecordType { get; }
    public string Version { get; }
    public IReadOnlyList<FormSection> Sections { get; }

    /// <summary>Every page in form order: section order, then page order.</summary>
    public IReadOnlyList<FormPage> Pages { get; }

    /// <summary>Every task in form order: section order, then task order.</summary>
    public IReadOnlyList<FormTask> Tasks => Sections.SelectMany(static s => s.Tasks).ToArray();

    public FormPage FirstPage => Pages[0];

    public FormModel(RecordType recordType, string version, IReadOnlyList<FormSection> sections)
    {
        RecordType = recordType;
        Version = version;
        Sections = sections;
        Pages = sections.SelectMany(static s => s.Tasks).SelectMany(static t => t.Pages).ToArray();
        _pageIndex = Pages
            .Select(static (page, index) => (page.Id, index))
            .ToDictionary(static x => x.Id, static x => x.index, StringComparer.Ordinal);
    }

    public FormPage? FindPage(string pageId) =>
        _pageIndex.TryGetValue(pageId, out int index) ? Pages[index] : null;

    public FormPage? NextPage(FormPage page)
    {
        int index = _pageIndex[page.Id] + 1;
        return index < Pages.Count ? Pages[index] : null;
    }

    public FormPage? PreviousPage(FormPage page)
    {
        int index = _pageIndex[page.Id] - 1;
        return index >= 0 ? Pages[index] : null;
    }
}
