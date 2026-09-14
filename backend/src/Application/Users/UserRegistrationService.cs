using Amazon.SimpleSystemsManagement.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.InternalServices.UserOnboarding;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users;

internal class UserRegistrationService : IUserRegistrationService
{
    private readonly IOrganisationAuthoriser _organisationAuthoriser;
    private readonly UserOnboardingService _userOnboardingService;
    private readonly ICurrentUserInfoService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ISetupLinkCreator _setupLinkCreator;
    private readonly AppDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserRegistrationService(
        AppDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IOrganisationAuthoriser organisationAuthoriser,
        UserOnboardingService userOnboardingService,
        ICurrentUserInfoService currentUserService,
        IEmailService emailService,
        ISetupLinkCreator setupLinkCreator
    )
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _organisationAuthoriser = organisationAuthoriser;
        _userOnboardingService = userOnboardingService;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _setupLinkCreator = setupLinkCreator;
    }

    public async Task<Result<RegisterUserConfirmationDto, RegisterUserError>> RegisterUser(
        int organisationId,
        RegisterUserCommandDto registerUserCommandDto,
        CancellationToken cancellationToken
    )
    {
        bool organisationExists = await _dbContext.Organisations.AnyAsync(
            o => o.Id == organisationId && o.Status == UserOrgStatus.Active,
            cancellationToken
        );
        if (organisationExists)
        {
            var userRegister = new UserRegistrationRequest
            {
                OrganisationId = organisationId,
                FullName = registerUserCommandDto.FullName,
                PhoneNumber = registerUserCommandDto.PhoneNumber,
                WorkEmail = registerUserCommandDto.WorkEmail,
                CreatedAt = _dateTimeProvider.GetUtcNow(),
            };
            _dbContext.UserRegistrationRequests.Add(userRegister);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var request = await _dbContext
                .UserRegistrationRequests.AsNoTracking()
                .Include(x => x.Organisation)
                .SingleAsync(x => x.Id == userRegister.Id, cancellationToken);

            var dto = MapToDto(request);

            return Result<RegisterUserConfirmationDto, RegisterUserError>.Ok(dto);
        }
        return Result<RegisterUserConfirmationDto, RegisterUserError>.Err(
            new RegisterUserError.OrganisationNotFound()
        );
    }

    public async Task<GetUserRegistrationByIdResult> GetUserRegistrationById(
        int organisationId,
        int id,
        CancellationToken cancellationToken
    )
    {
        var request = await _dbContext
            .UserRegistrationRequests.AsNoTracking()
            .Include(x => x.Organisation)
            .Where(x => x.OrganisationId == organisationId && x.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        if (request is null)
        {
            return GetUserRegistrationByIdResult.Err(new GetUserDetailsError.IdNotFound(id));
        }
        var authorised = _organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.SignUpUser,
            request.OrganisationId
        );
        if (!authorised)
        {
            return GetUserRegistrationByIdResult.Err(new GetUserDetailsError.UserNotAuthorised());
        }
        var dto = MapToDto(request);
        return GetUserRegistrationByIdResult.Ok(dto);
    }

    public async Task<Result<ApproveRequestError>> ApproveRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        bool isAuthorised = _organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.Update,
            organisationId
        );
        if (!isAuthorised)
        {
            return Result<ApproveRequestError>.Err(new ApproveRequestError.NotAllowed());
        }
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        UserRegistrationRequest? registrationRequest =
            await _dbContext.UserRegistrationRequests.FirstOrDefaultAsync(
                x => x.OrganisationId == organisationId && x.Id == registrationRequestId,
                cancellationToken
            );
        if (registrationRequest is null)
        {
            return Result<ApproveRequestError>.Err(new ApproveRequestError.RequestNotFound());
        }

        Result<ApproveRequestError>? validStateResult = CheckRegistrationUpdateResult(
            registrationRequest
        );

        if (validStateResult is not null)
        {
            return validStateResult.Value;
        }

        var currentUser = await GetCurrentUser(cancellationToken);
        registrationRequest.Approve(currentUser, _dateTimeProvider.GetUtcNow());

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<ApproveRequestError>.Err(new ApproveRequestError.ConcurrencyError());
        }

        var result = await _userOnboardingService.InitialiseNewUserSetup(
            new OnboardUserCommandDto()
            {
                FullName = registrationRequest.FullName,
                NewUserEmail = registrationRequest.WorkEmail,
                OrganisationId = registrationRequest.OrganisationId,
                ContactNumber = registrationRequest.PhoneNumber,
            },
            cancellationToken
        );
        return await result.Match(
            targetUser => HandleOnboardingSuccess(transaction, targetUser, cancellationToken),
            err => HandleUserOnBoardingError(transaction, err, cancellationToken)
        );
    }

    public async Task<Result<RejectRequestError>> RejectRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        bool isAuthorised = _organisationAuthoriser.CanPerformOperationOnOrganisation(
            Operation.Update,
            organisationId
        );
        if (!isAuthorised)
        {
            return Result<RejectRequestError>.Err(new RejectRequestError.NotAllowed());
        }
        var registrationRequest = await _dbContext.UserRegistrationRequests.FirstOrDefaultAsync(
            x => x.OrganisationId == organisationId && x.Id == registrationRequestId,
            cancellationToken
        );
        if (registrationRequest is null)
        {
            return Result<RejectRequestError>.Err(new RejectRequestError.RequestNotFound());
        }

        Result<RejectRequestError>? validStateResult = registrationRequest.GetState() switch
        {
            UserRegistrationRequest.State.Approved => Result<RejectRequestError>.Err(
                new RejectRequestError.RegistrationApproved()
            ),
            UserRegistrationRequest.State.Rejected => Result<RejectRequestError>.Ok(),
            _ => null,
        };
        if (validStateResult is not null)
        {
            return validStateResult.Value;
        }

        var currentUser = await GetCurrentUser(cancellationToken);
        registrationRequest.Reject(currentUser, _dateTimeProvider.GetUtcNow());

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<RejectRequestError>.Err(new RejectRequestError.ConcurrencyError());
        }
        await _emailService.SendEmail(
            new()
            {
                PersonIdentifier = PersonIdentifier.FromRegistrationId(registrationRequest.Id),
                RecipientAddress = registrationRequest.WorkEmail,
                Email = new UserMembershipRequestRejectedNotificationEmail(),
            },
            cancellationToken
        );
        return Result<RejectRequestError>.Ok();
    }

    private static Result<ApproveRequestError>? CheckRegistrationUpdateResult(
        UserRegistrationRequest registrationRequest
    )
    {
        return registrationRequest.GetState() switch
        {
            UserRegistrationRequest.State.Approved => Result<ApproveRequestError>.Ok(),
            UserRegistrationRequest.State.Rejected => Result<ApproveRequestError>.Err(
                new ApproveRequestError.RegistrationRejected()
            ),
            _ => null,
        };
    }

    private async Task<Result<ApproveRequestError>> HandleOnboardingSuccess(
        IDbContextTransaction transaction,
        User targetUser,
        CancellationToken cancellationToken
    )
    {
        Uri link = _setupLinkCreator.GetSetupLink(targetUser.OnboardingRecord!.SetupToken);
        await _emailService.SendEmail(
            new()
            {
                RecipientAddress = targetUser.WorkEmail,
                PersonIdentifier = targetUser.CognitoUsername,
                Email = new UserMembershipRequestApprovedNotificationEmail() { Link = link },
            },
            cancellationToken
        );
        await transaction.CommitAsync(cancellationToken);
        return Result<ApproveRequestError>.Ok();
    }

    private static async Task<Result<ApproveRequestError>> HandleUserOnBoardingError(
        IDbContextTransaction transaction,
        OnboardUserError err,
        CancellationToken cancellationToken
    )
    {
        await transaction.RollbackAsync(cancellationToken);
        return err.Match(
            usernameAlreadyExists: x =>
                throw new InvalidOperationException("The generated username already exists."),
            invalidOrganisation: x =>
                Result<ApproveRequestError>.Err(new ApproveRequestError.InvalidOrganisation()),
            notAllowed: x => Result<ApproveRequestError>.Err(new ApproveRequestError.NotAllowed()),
            userAlreadyExists: _ =>
                Result<ApproveRequestError>.Err(new ApproveRequestError.UserAlreadyExists())
        );
    }

    private async Task<User> GetCurrentUser(CancellationToken cancellationToken)
    {
        CurrentUser currentUserInfo = _currentUserService.GetCurrentUserInfo();
        User? foundValue = await _dbContext.Users.FirstOrDefaultAsync(
            x => x.CognitoUsername == currentUserInfo.CognitoUsername,
            cancellationToken
        );
        return foundValue
            ?? throw new InvalidOptionException(
                $"Could not find specified current user in the database. [{currentUserInfo.CognitoUsername}]"
            );
    }

    private static RegisterUserConfirmationDto MapToDto(
        UserRegistrationRequest userRegistrationRequest
    )
    {
        return new()
        {
            Id = userRegistrationRequest.Id,
            OrganisationName = userRegistrationRequest.Organisation!.OrganisationName,
            FullName = userRegistrationRequest.FullName,
            WorkEmail = userRegistrationRequest.WorkEmail,
            PhoneNumber = userRegistrationRequest.PhoneNumber,
        };
    }
}
