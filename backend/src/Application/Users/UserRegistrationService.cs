using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Users;

internal class UserRegistrationService : IUserRegistrationService
{
    private readonly IOrganisationAuthoriser _organisationAuthoriser;
    private readonly AppDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserRegistrationService(
        AppDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IOrganisationAuthoriser organisationAuthoriser
    )
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _organisationAuthoriser = organisationAuthoriser;
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

    public Task<Result<ApproveRequestError>> ApproveRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public Task<Result<RejectRequestError>> RejectRequest(
        int organisationId,
        int registrationRequestId,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
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
