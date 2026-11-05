namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when saving a record content page.</summary>
public abstract record SaveRecordPageError
{
    private SaveRecordPageError() { }

    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NotAllowed, TResult> notAllowed,
        Func<CannotEdit, TResult> cannotEdit,
        Func<NotEditable, TResult> notEditable,
        Func<FormVersionChanged, TResult> formVersionChanged,
        Func<ConcurrentUpdate, TResult> concurrentUpdate,
        Func<PageAnswersChanged, TResult> pageAnswersChanged,
        Func<Invalid, TResult> invalid
    )
    {
        return this switch
        {
            NotFound e => notFound(e),
            NotAllowed e => notAllowed(e),
            CannotEdit e => cannotEdit(e),
            NotEditable e => notEditable(e),
            FormVersionChanged e => formVersionChanged(e),
            ConcurrentUpdate e => concurrentUpdate(e),
            PageAnswersChanged e => pageAnswersChanged(e),
            Invalid e => invalid(e),
            _ => throw new InvalidOperationException(
                $"Unexpected {nameof(SaveRecordPageError)} case: {GetType().Name}"
            ),
        };
    }

    /// <summary>The record, revision or page does not exist.</summary>
    public sealed record NotFound : SaveRecordPageError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : SaveRecordPageError;

    /// <summary>The caller can read the record but not edit its content.</summary>
    public sealed record CannotEdit : SaveRecordPageError;

    /// <summary>
    /// The latest revision is not a draft that can be edited: it has been submitted, or the record
    /// is on hold or archived.
    /// </summary>
    public sealed record NotEditable : SaveRecordPageError;

    /// <summary>The form has changed since the page was loaded.</summary>
    public sealed record FormVersionChanged : SaveRecordPageError;

    /// <summary>
    /// Another save to the same record revision was committed at the same moment as this one.
    /// </summary>
    public sealed record ConcurrentUpdate : SaveRecordPageError;

    /// <summary>The answers on this page have changed since the page was loaded.</summary>
    public sealed record PageAnswersChanged : SaveRecordPageError;

    /// <summary>One or more answers are invalid. Messages are keyed by question ID.</summary>
    public sealed record Invalid(IReadOnlyDictionary<string, string> Errors) : SaveRecordPageError;
}
