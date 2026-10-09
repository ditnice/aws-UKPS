using UKPS.Api.Application.Common;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;

namespace UKPS.Api.Application.Forms;

/// <summary>
/// Retrieves pages of a record's content form.
/// </summary>
public interface IRecordPageQueryService
{
    /// <summary>
    /// Gets a page of a record revision's content form, with its current answers.
    /// </summary>
    /// <param name="recordId">The record's identifier.</param>
    /// <param name="revisionId">The revision's identifier; must belong to the record.</param>
    /// <param name="pageId">The page's identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The page, or the reason it could not be retrieved.</returns>
    Task<Result<RecordPageDto, GetRecordPageError>> GetPage(
        int recordId,
        int revisionId,
        string pageId,
        CancellationToken cancellationToken
    );
}
