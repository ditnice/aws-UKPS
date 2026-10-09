namespace UKPS.Api.Application.Forms.Errors;

/// <summary>
/// Represents an error that can occur when saving a page of a record's content form.
/// </summary>
public abstract record SaveRecordPageError
{
    /// <summary>The record or revision does not exist.</summary>
    public sealed record RecordNotFound : SaveRecordPageError;

    /// <summary>The caller may not read the record.</summary>
    public sealed record NotAllowed : SaveRecordPageError;

    /// <summary>The record's form has no such page.</summary>
    public sealed record PageNotFound : SaveRecordPageError;

    /// <summary>The caller may read the record but not edit its content.</summary>
    public sealed record EditNotAllowed : SaveRecordPageError;

    /// <summary>The revision is not a draft, so its content cannot change.</summary>
    public sealed record RevisionNotDraft : SaveRecordPageError;

    /// <summary>
    /// The revision changed since the page was loaded (or is changing concurrently).
    /// </summary>
    public sealed record RevisionChanged : SaveRecordPageError;

    /// <summary>The form definition changed since the page was loaded.</summary>
    public sealed record FormVersionChanged : SaveRecordPageError;

    /// <summary>
    /// The answers are invalid. Errors are keyed by question ID (or by the unknown key).
    /// </summary>
    /// <param name="Errors">The error messages for each invalid answer.</param>
    public sealed record Invalid(IReadOnlyDictionary<string, string[]> Errors)
        : SaveRecordPageError;

    internal TResult Match<TResult>(
        Func<RecordNotFound, TResult> recordNotFound,
        Func<NotAllowed, TResult> notAllowed,
        Func<PageNotFound, TResult> pageNotFound,
        Func<EditNotAllowed, TResult> editNotAllowed,
        Func<RevisionNotDraft, TResult> revisionNotDraft,
        Func<RevisionChanged, TResult> revisionChanged,
        Func<FormVersionChanged, TResult> formVersionChanged,
        Func<Invalid, TResult> invalid
    )
    {
        ArgumentNullException.ThrowIfNull(recordNotFound);
        ArgumentNullException.ThrowIfNull(notAllowed);
        ArgumentNullException.ThrowIfNull(pageNotFound);
        ArgumentNullException.ThrowIfNull(editNotAllowed);
        ArgumentNullException.ThrowIfNull(revisionNotDraft);
        ArgumentNullException.ThrowIfNull(revisionChanged);
        ArgumentNullException.ThrowIfNull(formVersionChanged);
        ArgumentNullException.ThrowIfNull(invalid);

        return this switch
        {
            RecordNotFound e => recordNotFound(e),
            NotAllowed e => notAllowed(e),
            PageNotFound e => pageNotFound(e),
            EditNotAllowed e => editNotAllowed(e),
            RevisionNotDraft e => revisionNotDraft(e),
            RevisionChanged e => revisionChanged(e),
            FormVersionChanged e => formVersionChanged(e),
            Invalid e => invalid(e),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(SaveRecordPageError)} subtype: {GetType().Name}."
            ),
        };
    }
}
