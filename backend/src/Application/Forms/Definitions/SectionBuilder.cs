namespace UKPS.Api.Application.Forms.Definitions;

internal sealed class SectionBuilder(string sectionId, string sectionTitle)
{
    public List<PageDefinition> Pages { get; } = [];

    public SectionBuilder Page(string id, string title, Action<PageBuilder> configure)
    {
        var builder = new PageBuilder();
        configure(builder);
        Pages.Add(new PageDefinition(id, title, sectionId, sectionTitle, builder.Questions));
        return this;
    }
}
