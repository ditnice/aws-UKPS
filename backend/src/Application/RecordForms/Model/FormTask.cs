namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>A group of ordered pages within a section.</summary>
internal sealed record FormTask(string Id, string Title, int Order, IReadOnlyList<FormPage> Pages)
{
    public FormPage FirstPage => Pages[0];

    public IReadOnlyList<FormQuestion> Questions =>
        Pages.SelectMany(static page => page.Questions).ToArray();
}
