namespace UKPS.Api.Application.RecordForms.Options;

internal enum OptionProviderKind
{
    /// <summary>A fixed list of enum members.</summary>
    Enum = 0,

    /// <summary>A flat list of reference data rows.</summary>
    Reference = 1,

    /// <summary>Two levels of reference data: parents, each with children.</summary>
    ReferenceTree = 2,
}
