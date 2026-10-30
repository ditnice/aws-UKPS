namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when saving a record content page.</summary>
public abstract record SaveRecordPageError
{
    /// <summary>The record, revision or page does not exist.</summary>
    public sealed record NotFound : SaveRecordPageError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : SaveRecordPageError;

    /// <summary>The caller can read the record but not edit its content.</summary>
    public sealed record CannotEdit : SaveRecordPageError;

    /// <summary>The revision is no longer a draft.</summary>
    public sealed record NotDraft : SaveRecordPageError;

    /// <summary>The form has changed since the page was loaded.</summary>
    public sealed record FormVersionChanged : SaveRecordPageError;

    /// <summary>The revision has changed since the page was loaded.</summary>
    public sealed record RevisionChanged : SaveRecordPageError;

    /// <summary>One or more answers are invalid. Messages are keyed by question ID.</summary>
    public sealed record Invalid(IReadOnlyDictionary<string, string> Errors) : SaveRecordPageError;
}
