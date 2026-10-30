namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Where a question's answer is stored, worked out from the EF model.</summary>
internal enum BindingKind
{
    /// <summary>A string column.</summary>
    Text = 0,

    /// <summary>A non-flags enum column.</summary>
    Enum = 1,

    /// <summary>An integer foreign key to reference data.</summary>
    ReferenceId = 2,

    /// <summary>A <see cref="FlagsAttribute"/> enum column.</summary>
    Flags = 3,

    /// <summary>A collection navigation to a junction entity.</summary>
    Junction = 4,
}
