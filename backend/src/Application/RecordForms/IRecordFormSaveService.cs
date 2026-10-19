global using SaveRecordPageResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.SaveRecordPageDto,
    UKPS.Api.Application.RecordForms.Errors.SaveRecordPageError
>;
using UKPS.Api.Application.RecordForms.Dtos;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Validates and saves record content form pages.</summary>
public interface IRecordFormSaveService
{
    /// <summary>
    /// Validates every answer on the page and writes the changed ones, with one audit event.
    /// </summary>
    Task<SaveRecordPageResult> SavePage(
        int recordId,
        int revisionId,
        string pageId,
        SaveRecordPageCommand command,
        CancellationToken cancellationToken
    );
}
