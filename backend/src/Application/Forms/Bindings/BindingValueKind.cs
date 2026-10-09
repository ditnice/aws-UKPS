namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>The kind of value a binding stores, used to check it suits its question type.</summary>
internal enum BindingValueKind
{
    /// <summary>A string column.</summary>
    Text = 0,

    /// <summary>An enum column; the answer is the enum member name.</summary>
    Enum = 1,

    /// <summary>An integer reference data foreign key; the answer is the ID as a string.</summary>
    ReferenceId = 2,

    /// <summary>A set of reference data IDs, e.g. a junction table.</summary>
    ReferenceIdList = 3,
}
