namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when loading a record content page.</summary>
public abstract record GetRecordPageError
{
    /// <summary>The record, revision or page does not exist.</summary>
    public sealed record NotFound : GetRecordPageError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : GetRecordPageError;
}
