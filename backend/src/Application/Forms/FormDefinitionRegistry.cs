using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Medicines;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Forms;

/// <summary>
/// The content form for each record type. Registered as a singleton built during service
/// registration, so an invalid definition stops the app starting.
/// </summary>
internal sealed class FormDefinitionRegistry
{
    private readonly Dictionary<RecordType, FormDefinition> _forms;

    public FormDefinitionRegistry(IEnumerable<FormDefinition> forms)
    {
        _forms = forms
            .Select(FormDefinitionValidator.EnsureValid)
            .ToDictionary(form => form.RecordType);
    }

    public static FormDefinitionRegistry CreateDefault() => new([MedicineFormDefinition.Create()]);

    /// <summary>The form for <paramref name="recordType"/>, or <c>null</c> if it has none yet.</summary>
    public FormDefinition? Get(RecordType recordType) => _forms.GetValueOrDefault(recordType);
}
