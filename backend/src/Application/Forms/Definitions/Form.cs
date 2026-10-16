using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// Fluent entry point for declaring a form, e.g.
/// <c>Form.For(RecordType.Medicine, "2026.10.1").Section(...).Build()</c>.
/// </summary>
internal static class Form
{
    public static FormBuilder For(RecordType recordType, string version) =>
        new(recordType, version);
}
