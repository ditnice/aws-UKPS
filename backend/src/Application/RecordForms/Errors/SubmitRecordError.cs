using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when submitting a record.</summary>
public abstract record SubmitRecordError
{
    private SubmitRecordError() { }

    internal TResult Match<TResult>(
        Func<NotFound, TResult> notFound,
        Func<NotAllowed, TResult> notAllowed,
        Func<CannotSubmit, TResult> cannotSubmit,
        Func<NotSubmittable, TResult> notSubmittable,
        Func<FormVersionChanged, TResult> formVersionChanged,
        Func<RecordChanged, TResult> recordChanged,
        Func<Invalid, TResult> invalid
    )
    {
        return this switch
        {
            NotFound e => notFound(e),
            NotAllowed e => notAllowed(e),
            CannotSubmit e => cannotSubmit(e),
            NotSubmittable e => notSubmittable(e),
            FormVersionChanged e => formVersionChanged(e),
            RecordChanged e => recordChanged(e),
            Invalid e => invalid(e),
            _ => throw new InvalidOperationException(
                $"Unexpected {nameof(SubmitRecordError)} case: {GetType().Name}"
            ),
        };
    }

    /// <summary>The record does not exist, or has no form.</summary>
    public sealed record NotFound : SubmitRecordError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : SubmitRecordError;

    /// <summary>The caller can read the record but not submit it.</summary>
    public sealed record CannotSubmit : SubmitRecordError;

    /// <summary>
    /// The latest revision is not a draft that can be submitted: it has already been submitted, or
    /// the record is on hold or archived.
    /// </summary>
    public sealed record NotSubmittable : SubmitRecordError;

    /// <summary>The form has changed since check your answers was loaded.</summary>
    public sealed record FormVersionChanged : SubmitRecordError;

    /// <summary>
    /// The revision has changed since check your answers was loaded, or changed while it was
    /// being submitted.
    /// </summary>
    public sealed record RecordChanged : SubmitRecordError;

    /// <summary>The revision has problems that must be fixed first, in form order.</summary>
    public sealed record Invalid(IReadOnlyList<SubmissionErrorDto> Errors) : SubmitRecordError;
}
