using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>The content form of one record type: ordered sections, each with ordered pages.</summary>
internal sealed class FormModel
{
    private readonly Dictionary<string, int> _pageIndex;

    public RecordType RecordType { get; }
    public string Version { get; }
    public IReadOnlyList<FormSection> Sections { get; }

    /// <summary>Every page in form order: section order, then task order, then page order.</summary>
    public IReadOnlyList<FormPage> Pages { get; }

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

    /// <summary>The page after this one in its task, or null on the task's last page.</summary>
    public FormPage? NextPage(FormPage page) => PageInSameTask(page, _pageIndex[page.Id] + 1);

    /// <summary>The page before this one in its task, or null on the task's first page.</summary>
    public FormPage? PreviousPage(FormPage page) => PageInSameTask(page, _pageIndex[page.Id] - 1);

    // Pages are in task order, so a task's pages are adjacent.
    private FormPage? PageInSameTask(FormPage page, int index) =>
        index >= 0
        && index < Pages.Count
        && string.Equals(Pages[index].TaskId, page.TaskId, StringComparison.Ordinal)
            ? Pages[index]
            : null;
}
