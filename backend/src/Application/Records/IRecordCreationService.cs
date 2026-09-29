global using CreateRecordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.CreateRecordDto,
    UKPS.Api.Application.Records.Errors.CreateRecordError
>;
using UKPS.Api.Application.Records.Dtos;

namespace UKPS.Api.Application.Records;

/// <summary>
/// Provides functionality for creating records.
/// </summary>
public interface IRecordCreationService
{
    /// <summary>
    /// Creates a new record for the specified organisation.
    /// </summary>
    /// <param name="command">
    /// The details of the record to create.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result indicates
    /// whether the record was created successfully or why creation failed.
    /// </returns>
    Task<CreateRecordResult> CreateRecord(
        CreateRecordCommand command,
        CancellationToken cancellationToken
    );
}
