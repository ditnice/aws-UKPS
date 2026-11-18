namespace UKPS.Api.Application.RecordForms;

/// <summary>Why the caller can't submit a record's latest revision, whatever its answers.</summary>
internal enum SubmitBlocker
{
    /// <summary>The caller can read the record but not submit it.</summary>
    CannotSubmit = 0,

    /// <summary>
    /// The latest revision is not a draft that can be submitted: it has already been submitted, or
    /// the record is on hold or archived.
    /// </summary>
    NotSubmittable = 1,
}
