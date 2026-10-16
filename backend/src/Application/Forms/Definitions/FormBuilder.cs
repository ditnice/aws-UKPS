using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Forms.Definitions;

internal sealed class FormBuilder(RecordType recordType, string version)
{
    private readonly List<SectionDefinition> _sections = [];

    public FormBuilder Section(string id, string title, Action<SectionBuilder> configure)
    {
        var builder = new SectionBuilder(id, title);
        configure(builder);
        _sections.Add(new SectionDefinition(id, title, builder.Pages));
        return this;
    }

    public FormDefinition Build() => new(recordType, version, _sections);
}
