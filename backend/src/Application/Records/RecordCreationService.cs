using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.RecordForms;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

internal class RecordCreationService : IRecordCreationService
{
    private readonly CurrentDbUserEntityService _currentDbUserEntityService;
    private readonly AppDbContext _dbContext;
    private readonly IOrganisationAuthoriser _organisationAuthoriser;
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly FormModelRegistry _formModels;

    public RecordCreationService(
        CurrentDbUserEntityService currentDbUserEntityService,
        AppDbContext dbContext,
        IOrganisationAuthoriser organisationAuthoriser,
        ICurrentUserInfoService currentUserInfoService,
        IDateTimeProvider dateTimeProvider,
        FormModelRegistry formModels
    )
    {
        _formModels = formModels;
        _currentDbUserEntityService = currentDbUserEntityService;
        _dbContext = dbContext;
        _organisationAuthoriser = organisationAuthoriser;
        _currentUserInfoService = currentUserInfoService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CreateRecordResult> CreateRecord(
        CreateRecordCommand command,
        CancellationToken cancellationToken
    )
    {
        _currentUserInfoService.ThrowIfOrganisationNotSelected(command.OrganisationId);

        bool isAuthorised = _organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.Create,
            command.OrganisationId
        );
        if (!isAuthorised)
        {
            return CreateRecordResult.Err(new CreateRecordError.NotAuthorised());
        }

        Organisation? organisation = await _dbContext
            .Organisations.Where(x => x.Status == UserOrgStatus.Active)
            .FirstOrDefaultAsync(x => x.Id == command.OrganisationId, cancellationToken);
        if (organisation is null)
        {
            return CreateRecordResult.Err(new CreateRecordError.OrganisationDoesNotExist());
        }

        DateTime time = _dateTimeProvider.GetUtcNow();
        User currentUser = await _currentDbUserEntityService.GetCurrentUser(cancellationToken);

        (Record record, RecordRevision revision) = Record.CreateInitial(
            organisation,
            time,
            currentUser
        );
        RecordProductDetail recordProductDetail = new RecordProductDetail()
        {
            CompanyCode = command.CompanyCode,
            RecordTitle = command.RecordTitle,
            BrandedName = command.BrandedName,
            NamesAndIdentifiers = CreateNamesAndIdentifiersArray(command),
            Revision = revision,
        };

        await _dbContext.AddAsync(recordProductDetail, cancellationToken);
        _dbContext.RecordFieldAudits.AddRange(
            CreateFieldAudits(command, record, revision, time, currentUser)
        );
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CreateRecordResult.Ok(
            new CreateRecordDto()
            {
                RecordId = record.Id,
                FirstPageId = _organisationAuthoriser.CanPerformOperationOnOrganisation(
                    Operation.EditRecordContent,
                    command.OrganisationId
                )
                    ? _formModels.Find(record.RecordType)?.FirstPage.Id
                    : null,
            }
        );
    }

    private static RecordNameAndIdentifier[] CreateNamesAndIdentifiersArray(
        CreateRecordCommand command
    )
    {
        var genericNames = command.GenericNames.Select(
            static (x, index) =>
                new RecordNameAndIdentifier
                {
                    Name = x,
                    NameType = NameAndIdentifierType.GenericName,
                    DisplayOrder = index + 1,
                }
        );
        var otherIdentifiers = command.OtherIdentifiers.Select(
            static (x, index) =>
                new RecordNameAndIdentifier
                {
                    Name = x,
                    NameType = NameAndIdentifierType.OtherIdentifier,
                    DisplayOrder = index + 1,
                }
        );
        return genericNames.Concat(otherIdentifiers).ToArray();
    }

    /// <summary>
    /// Audits each field answered on the creation page as a change from null. Unanswered fields
    /// are not audited.
    /// </summary>
    private static RecordFieldAudit[] CreateFieldAudits(
        CreateRecordCommand command,
        Record record,
        RecordRevision revision,
        DateTime time,
        User currentUser
    )
    {
        (string FieldPath, string? Value)[] fields =
        [
            ("record_product_detail.company_code", command.CompanyCode),
            ("record_product_detail.branded_name", command.BrandedName),
            ("record_product_detail.record_title", command.RecordTitle),
            (
                "record_name_and_identifier",
                RecordNameAndIdentifier.AuditText(command.GenericNames, command.OtherIdentifiers)
            ),
        ];
        return fields
            .Where(static field => field.Value is not null)
            .Select(field => new RecordFieldAudit
            {
                Record = record,
                Revision = revision,
                FieldPath = field.FieldPath,
                OldValue = null,
                NewValue = field.Value,
                ChangedByUser = currentUser,
                ChangedAt = time,
            })
            .ToArray();
    }
}
