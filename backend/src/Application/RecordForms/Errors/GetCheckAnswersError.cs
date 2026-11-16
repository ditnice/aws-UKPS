namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when loading check your answers.</summary>
public abstract record GetCheckAnswersError
{
    /// <summary>The record does not exist, or has no form.</summary>
    public sealed record NotFound : GetCheckAnswersError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : GetCheckAnswersError;
}
