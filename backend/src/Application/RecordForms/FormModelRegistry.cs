using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.RecordForms;

/// <summary>The validated form model of each record type that has one. A singleton.</summary>
internal sealed class FormModelRegistry(IReadOnlyDictionary<RecordType, FormModel> models)
{
    public IEnumerable<FormModel> Models => models.Values;

    public FormModel? Find(RecordType recordType) => models.GetValueOrDefault(recordType);
}
