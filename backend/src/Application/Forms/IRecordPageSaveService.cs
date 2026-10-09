using UKPS.Api.Application.Common;
using UKPS.Api.Application.Forms.Dtos;
using UKPS.Api.Application.Forms.Errors;

namespace UKPS.Api.Application.Forms;

/// <summary>
/// Saves pages of a record's content form.
/// </summary>
public interface IRecordPageSaveService
{
    /// <summary>
    /// Validates and saves every answer on a page of a draft revision, audits the changes and
    /// returns the next page.
    /// </summary>
    /// <param name="recordId">The record's identifier.</param>
    /// <param name="revisionId">The revision's identifier; must belong to the record.</param>
    /// <param name="pageId">The page's identifier.</param>
    /// <param name="command">The page's answers and the versions it was loaded with.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The next page, or the reason the page could not be saved.</returns>
    Task<Result<SaveRecordPageDto, SaveRecordPageError>> SavePage(
        int recordId,
        int revisionId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    );
}
