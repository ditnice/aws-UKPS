using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
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
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecordCreationService(
        CurrentDbUserEntityService currentDbUserEntityService,
        AppDbContext dbContext,
        IOrganisationAuthoriser organisationAuthoriser,
        IDateTimeProvider dateTimeProvider
    )
    {
        _currentDbUserEntityService = currentDbUserEntityService;
        _dbContext = dbContext;
        _organisationAuthoriser = organisationAuthoriser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CreateRecordResult> CreateRecord(
        CreateRecordCommand command,
        CancellationToken cancellationToken
    )
    {
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
            CompanyCode = command.DevelopmentName,
            RecordTitle = command.RecordTitle,
            BrandedName = command.BrandedName,
            NamesAndIdentifiers = CreateNamesAndIdentifiersArray(command),
            Revision = revision,
        };

        await _dbContext.AddAsync(recordProductDetail, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CreateRecordResult.Ok(
            new CreateRecordDto() { RecordId = record.Id, RevisionId = revision.Id }
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
}
