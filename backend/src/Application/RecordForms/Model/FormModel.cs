using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>The content form of one record type: ordered sections, each with ordered pages.</summary>
internal sealed class FormModel
{
    private readonly Dictionary<string, int> _pageIndex;
    private readonly Dictionary<string, FormQuestion> _questions;
    private readonly Dictionary<string, FormPage> _questionPages;
    private readonly Dictionary<string, PageScope> _scopes;

    public RecordType RecordType { get; }
    public string Version { get; }
    public IReadOnlyList<FormSection> Sections { get; }

    /// <summary>Every page in form order: section order, then task order, then page order.</summary>
    public IReadOnlyList<FormPage> Pages { get; }

    /// <summary>Every question in form order.</summary>
    public IReadOnlyList<FormQuestion> Questions { get; }

    /// <summary>
    /// The entities holding a question that any condition reads. Their answers are all that is
    /// needed to work out which of the form's pages and questions apply. Empty for a form without
    /// conditions.
    /// </summary>
    public IReadOnlyList<FormEntity> TriggerEntities { get; }

    public FormModel(RecordType recordType, string version, IReadOnlyList<FormSection> sections)
    {
        RecordType = recordType;
        Version = version;
        Sections = sections;
        Pages = sections.SelectMany(static s => s.Tasks).SelectMany(static t => t.Pages).ToArray();
        _pageIndex = Pages
            .Select(static (page, index) => (page.Id, index))
            .ToDictionary(static x => x.Id, static x => x.index, StringComparer.Ordinal);
        Questions = [.. Pages.SelectMany(static p => p.Questions)];
        _questions = Questions.ToDictionary(static q => q.Id, StringComparer.Ordinal);
        _questionPages = Pages
            .SelectMany(static p => p.Questions.Select(q => (q.Id, Page: p)))
            .ToDictionary(static x => x.Id, static x => x.Page, StringComparer.Ordinal);
        _scopes = BuildScopes();
        TriggerEntities = Questions
            .SelectMany(TriggersOf)
            .Select(static q => q.Entity)
            .Distinct()
            .ToArray();
    }

    public FormPage? FindPage(string pageId) =>
        _pageIndex.TryGetValue(pageId, out int index) ? Pages[index] : null;

    public FormQuestion? FindQuestion(string questionId) =>
        _questions.GetValueOrDefault(questionId);

    /// <summary>The page a question is on.</summary>
    public FormPage PageOf(FormQuestion question) => _questionPages[question.Id];

    /// <summary>
    /// The next page in the task that applies, or null when none does, where saving returns to the
    /// task list.
    /// </summary>
    public FormPage? NextPage(FormPage page, Applicability applicability) =>
        Pages
            .Skip(_pageIndex[page.Id] + 1)
            .TakeWhile(p => SameTask(p, page))
            .FirstOrDefault(applicability.Applies);

    /// <summary>The previous page in the task that applies, or null when none does.</summary>
    public FormPage? PreviousPage(FormPage page, Applicability applicability) =>
        Pages
            .Take(_pageIndex[page.Id])
            .Reverse()
            .TakeWhile(p => SameTask(p, page))
            .FirstOrDefault(applicability.Applies);

    // Pages are in task order, so a task's pages are adjacent.
    private static bool SameTask(FormPage a, FormPage b) =>
        string.Equals(a.TaskId, b.TaskId, StringComparison.Ordinal);

    /// <summary>
    /// The entities whose rows loading the page needs: its own, plus those holding the triggers
    /// of page conditions anywhere in the form, which decide whether the page applies and where
    /// Back and Save and continue go. A form without page conditions needs only the page's own.
    /// </summary>
    public IReadOnlyList<FormEntity> EntitiesToRead(FormPage page) =>
        _scopes[page.Id].EntitiesToRead;

    /// <summary>
    /// The entities whose rows saving the page needs: those it reads, plus those holding
    /// <see cref="AffectedBy"/>, whose answers the save may clear.
    /// </summary>
    public IReadOnlyList<FormEntity> EntitiesToSave(FormPage page) =>
        _scopes[page.Id].EntitiesToSave;

    /// <summary>
    /// The page's questions and every question elsewhere whose applicability depends on them,
    /// directly or through a chain of conditions. Saving the page can only change whether these
    /// apply, so they are the only answers the save may clear.
    /// </summary>
    public IReadOnlyList<FormQuestion> AffectedBy(FormPage page) => _scopes[page.Id].AffectedBy;

    /// <summary>The questions whose answers decide whether a question applies.</summary>
    private IEnumerable<FormQuestion> TriggersOf(FormQuestion question) =>
        new[] { PageOf(question).ShowWhen, question.ShowWhen }
            .OfType<FormCondition>()
            .SelectMany(static c => c.TriggerQuestionIds)
            .Select(id => _questions[id]);

    private Dictionary<string, PageScope> BuildScopes()
    {
        // Navigation reads the page-condition triggers, and the triggers those depend on.
        FormQuestion[] pageTriggers = Pages
            .Select(static p => p.ShowWhen)
            .OfType<FormCondition>()
            .SelectMany(static c => c.TriggerQuestionIds)
            .Select(id => _questions[id])
            .ToArray();
        FormEntity[] navigation = Closure(pageTriggers, TriggersOf)
            .Select(static q => q.Entity)
            .Distinct()
            .ToArray();

        ILookup<FormQuestion, FormQuestion> dependents = Questions
            .SelectMany(q => TriggersOf(q).Select(trigger => (Trigger: trigger, Dependent: q)))
            .ToLookup(static x => x.Trigger, static x => x.Dependent);

        return Pages.ToDictionary(
            static p => p.Id,
            page =>
            {
                FormQuestion[] affected = Closure(page.Questions, q => dependents[q]).ToArray();
                FormEntity[] read = page.Entities.Union(navigation).ToArray();
                FormEntity[] save = read.Union(affected.Select(static q => q.Entity)).ToArray();
                return new PageScope(read, save, affected);
            },
            StringComparer.Ordinal
        );
    }

    /// <summary>The starting questions and every question reachable from them, in form order.</summary>
    private FormQuestion[] Closure(
        IEnumerable<FormQuestion> start,
        Func<FormQuestion, IEnumerable<FormQuestion>> next
    )
    {
        HashSet<FormQuestion> seen = [];
        Stack<FormQuestion> pending = new(start);
        while (pending.TryPop(out FormQuestion? question))
        {
            if (seen.Add(question))
            {
                foreach (FormQuestion other in next(question))
                {
                    pending.Push(other);
                }
            }
        }

        return Questions.Where(seen.Contains).ToArray();
    }

    private sealed record PageScope(
        IReadOnlyList<FormEntity> EntitiesToRead,
        IReadOnlyList<FormEntity> EntitiesToSave,
        IReadOnlyList<FormQuestion> AffectedBy
    );
}
