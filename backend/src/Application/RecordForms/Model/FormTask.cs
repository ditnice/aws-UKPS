namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A group of ordered pages within a section.</summary>
internal sealed record FormTask(string Id, string Title, int Order, IReadOnlyList<FormPage> Pages)
{
    public FormPage FirstPage => Pages[0];

    /// <summary>Every question in the task, in page order.</summary>
    public IReadOnlyList<FormQuestion> Questions { get; } =
        Pages.SelectMany(static page => page.Questions).ToArray();
}
