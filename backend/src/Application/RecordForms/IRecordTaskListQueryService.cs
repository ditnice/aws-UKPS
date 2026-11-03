global using GetRecordTaskListResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.RecordForms.Dtos.RecordTaskListDto,
    UKPS.Api.Application.RecordForms.Errors.GetRecordTaskListError
>;

namespace UKPS.Api.Application.RecordForms;

/// <summary>Serves the task list for a record's latest revision.</summary>
public interface IRecordTaskListQueryService
{
    /// <summary>Loads the sections, tasks, page links and database-computed statuses.</summary>
    Task<GetRecordTaskListResult> GetTaskList(int recordId, CancellationToken cancellationToken);
}
