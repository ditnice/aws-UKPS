namespace UKPS.Api.Application.Records.Errors;

/// <summary>
/// Represents an abstract base record for errors that can occur when retrieving the QA worklist.
/// </summary>
public abstract record GetQaWorklistError
{
    /// <summary>
    /// Represents an error indicating that the operation is not allowed, because the caller is not a super user.
    /// </summary>
    internal sealed record NotAllowed() : GetQaWorklistError;
}
