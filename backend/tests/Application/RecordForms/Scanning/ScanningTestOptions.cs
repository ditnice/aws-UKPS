using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Tests.Application.RecordForms.Scanning;

/// <summary>The default option providers, plus those only the scanning test entities use.</summary>
internal static class ScanningTestOptions
{
    /// <summary>Every <see cref="MhraProcedureType"/>, for <see cref="CodedEntity"/>.</summary>
    public const string ProcedureTypes = "test-procedure-types";

    public static OptionProviderRegistry Create()
    {
        OptionProviderRegistry registry = OptionProviderRegistry.CreateDefault();
        registry.AddReference<MhraProcedureType>(
            ProcedureTypes,
            q => q.OrderBy(x => x.Id),
            x => new ReferenceOptionRow(x.Id, x.Label, x.IsArchived, null)
        );
        return registry;
    }
}
