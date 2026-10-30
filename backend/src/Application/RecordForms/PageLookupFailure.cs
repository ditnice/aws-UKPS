namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Why a form or page could not be loaded. Both map to the same neutral 404 (ADR-004).
/// </summary>
internal enum PageLookupFailure
{
    NotFound = 0,
    NotAllowed = 1,
}
