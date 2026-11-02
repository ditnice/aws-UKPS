namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>How a column binding converts between the column and its string answer.</summary>
internal enum ColumnValueKind
{
    /// <summary>A string column, passed as-is.</summary>
    Text = 0,

    /// <summary>A non-flags enum column, as its member name.</summary>
    Enum = 1,

    /// <summary>An integer foreign key to reference data, as the ID in invariant digits.</summary>
    ReferenceId = 2,
}
