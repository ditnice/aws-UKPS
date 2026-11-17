namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when loading check your answers.</summary>
public abstract record GetCheckAnswersError
{
    private GetCheckAnswersError() { }

    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NotAllowed, TResult> notAllowed,
        Func<CannotSubmit, TResult> cannotSubmit,
        Func<NotSubmittable, TResult> notSubmittable
    )
    {
        return this switch
        {
            NotFound e => notFound(e),
            NotAllowed e => notAllowed(e),
            CannotSubmit e => cannotSubmit(e),
            NotSubmittable e => notSubmittable(e),
            _ => throw new InvalidOperationException(
                $"Unexpected {nameof(GetCheckAnswersError)} case: {GetType().Name}"
            ),
        };
    }

    /// <summary>The record does not exist, or has no form.</summary>
    public sealed record NotFound : GetCheckAnswersError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : GetCheckAnswersError;

    /// <summary>The caller can read the record but not submit it.</summary>
    public sealed record CannotSubmit : GetCheckAnswersError;

    /// <summary>
    /// The latest revision is not a draft that can be submitted: it has already been submitted, or
    /// the record is on hold or archived.
    /// </summary>
    public sealed record NotSubmittable : GetCheckAnswersError;
}
