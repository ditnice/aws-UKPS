namespace UKPS.Api.Application.Forms.Errors;

/// <summary>
/// Represents an error that can occur when retrieving a page of a record's content form.
/// </summary>
public abstract record GetRecordPageError
{
    /// <summary>The record or revision does not exist.</summary>
    public sealed record RecordNotFound : GetRecordPageError;

    /// <summary>The caller may not read the record.</summary>
    public sealed record NotAllowed : GetRecordPageError;

    /// <summary>The record's form has no such page.</summary>
    public sealed record PageNotFound : GetRecordPageError;

    internal TResult Match<TResult>(
        Func<RecordNotFound, TResult> recordNotFound,
        Func<NotAllowed, TResult> notAllowed,
        Func<PageNotFound, TResult> pageNotFound
    )
    {
        ArgumentNullException.ThrowIfNull(recordNotFound);
        ArgumentNullException.ThrowIfNull(notAllowed);
        ArgumentNullException.ThrowIfNull(pageNotFound);

        return this switch
        {
            RecordNotFound e => recordNotFound(e),
            NotAllowed e => notAllowed(e),
            PageNotFound e => pageNotFound(e),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(GetRecordPageError)} subtype: {GetType().Name}."
            ),
        };
    }
}
