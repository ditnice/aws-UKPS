global using GetRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.RecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.GetRecordPageError
>;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Serves record content form pages.</summary>
public interface IRecordFormQueryService
{
    /// <summary>Loads a page's questions, options and current answers.</summary>
    Task<GetRecordPageResult> GetPage(
        int recordId,
        int revisionId,
        string pageId,
        CancellationToken cancellationToken
    );
}
