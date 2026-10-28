namespace UKPS.Api.Application.RecordForms.Errors;

/// <summary>Represents an error that can occur when loading a record task list.</summary>
public abstract record GetRecordTaskListError
{
    /// <summary>The record or its latest revision does not exist.</summary>
    public sealed record NotFound : GetRecordTaskListError;

    /// <summary>The caller cannot read the record's organisation.</summary>
    public sealed record NotAllowed : GetRecordTaskListError;
}
