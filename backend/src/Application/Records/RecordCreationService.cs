using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

internal class RecordCreationService : IRecordCreationService
{
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private readonly AppDbContext _dbContext;
    private readonly IOrganisationAuthoriser _organisationAuthoriser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecordCreationService(
        ICurrentUserInfoService currentUserInfoService,
        AppDbContext dbContext,
        IOrganisationAuthoriser organisationAuthoriser,
        IDateTimeProvider dateTimeProvider
    )
    {
        _currentUserInfoService = currentUserInfoService;
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
        User currentUser = await GetCurrentUser(cancellationToken);

        RecordRevision revision = new RecordRevision()
        {
            CreatedAt = time,
            CreatedByUser = currentUser,
            WorkflowStatus = WorkflowStatus.Draft,
            Record = new Record()
            {
                OrganisationId = command.OrganisationId,
                CreatedAt = time,
                CreatedByUser = currentUser,
            },
        };

        revision.Record.UpdateStatus(RecordStatus.Unpublished, currentUser, time);

        RecordEvent recordEvent = new RecordEvent()
        {
            Record = revision.Record,
            Revision = revision,
            EventType = RecordEventType.RecordCreated,
            PerformedAt = time,
            PerformedByUser = currentUser,
        };
        MedicinesProductDetail medicinesProductDetail = new MedicinesProductDetail()
        {
            RecordTitle = command.RecordTitle,
            BrandedName = command.BrandedName,
            ActiveSubstances = CreateActiveSubstancesArray(command),
            Revision = revision,
        };
        _dbContext.AddRange(medicinesProductDetail, recordEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CreateRecordResult.Ok(
            new CreateRecordDto() { RecordId = revision.RecordId, RevisionId = revision.Id }
        );
    }

    public async Task<User> GetCurrentUser(CancellationToken cancellationToken)
    {
        CurrentUser currentUserInfo = _currentUserInfoService.GetCurrentUserInfo();
        User? user = await _dbContext.Users.FirstOrDefaultAsync(
            x => x.CognitoUsername == currentUserInfo.CognitoUsername,
            cancellationToken
        );
        return user
            ?? throw new InvalidOperationException(
                "Could not find the current user in the database."
            );
    }

    private static MedicinesActiveSubstance[] CreateActiveSubstancesArray(
        CreateRecordCommand command
    )
    {
        var developmentNames = command.DevelopmentNames.Select(
            static (x, index) =>
                new MedicinesActiveSubstance
                {
                    Name = x,
                    NameType = SubstanceNameType.DevelopmentName,
                    DisplayOrder = index,
                }
        );
        var genericNames = command.GenericNames.Select(
            static (x, index) =>
                new MedicinesActiveSubstance
                {
                    Name = x,
                    NameType = SubstanceNameType.GenericName,
                    DisplayOrder = index,
                }
        );
        return developmentNames.Concat(genericNames).ToArray();
    }
}
