using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using GetUsersResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Common.PaginatedResponseDto<UKPS.Api.Application.Users.Dtos.UserListItemDto>,
    UKPS.Api.Application.Users.Errors.GetUsersError
>;

namespace UKPS.Api.Application.Users;

internal sealed class UserService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IUserService
{
    public async Task<GetUsersResult> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses,
        CancellationToken cancellationToken
    )
    {
        if (organisationId.HasValue)
        {
            bool actionPermitted = organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                organisationId.Value
            );
            if (!actionPermitted)
            {
                return GetUsersResult.Err(new GetUsersError.NotAllowed(organisationId.Value));
            }
            bool organisationExists = await dbContext.Organisations.AnyAsync(
                o => o.Id == organisationId.Value,
                cancellationToken
            );

            if (!organisationExists)
            {
                return GetUsersResult.Err(
                    new GetUsersError.OrganisationNotFound(organisationId.Value)
                );
            }
        }

        var permittedOrganisationIds = organisationAuthoriser.GetAuthorisedOrganisations(
            Operation.Read
        );
        var organisationMemberships = ApplyFilters(
            dbContext.UserOrgMemberships.AsNoTracking(),
            permittedOrganisationIds,
            organisationId,
            statuses
        );

        int totalCount = await organisationMemberships.CountAsync(cancellationToken);

        List<UserListItemDto> items = await organisationMemberships
            .OrderBy(m => m.User!.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new UserListItemDto
            {
                UserId = m.User!.Id,
                EmailAddress = m.User.WorkEmail,
                Role = m.UserRole,
                Status = m.Status,
                // No user-level last-active source exists yet.
                LastActive = null,
            })
            .ToListAsync(cancellationToken);

        return GetUsersResult.Ok(
            new PaginatedResponseDto<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            }
        );
    }

    private static IQueryable<UserOrgMembership> ApplyFilters(
        IQueryable<UserOrgMembership> input,
        ValueOrAll<int> permittedOrganisationIds,
        int? organisationId,
        IReadOnlyCollection<UserOrgStatus> statuses
    )
    {
        IQueryable<UserOrgMembership> organisationMemberships = input.Where(
            permittedOrganisationIds.Contains<UserOrgMembership>(x => x.OrganisationId)
        );

        if (organisationId.HasValue)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                m.OrganisationId == organisationId.Value
            );
        }

        if (statuses.Count > 0)
        {
            organisationMemberships = organisationMemberships.Where(m =>
                statuses.Contains(m.Status)
            );
        }

        return organisationMemberships;
    }

    public async Task<Result<UserDetailsDto, CreateUserError>> CreateUser(
        CreateUserRequestDto createUserRequestDto,
        CancellationToken cancellationToken
    )
    {
        var organisation = await dbContext.Organisations.FindAsync(
            [createUserRequestDto.OrganisationId],
            cancellationToken
        );
        if (organisation is null)
        {
            return Result<UserDetailsDto, CreateUserError>.Err(
                new CreateUserError.NotFound(createUserRequestDto.OrganisationId)
            );
        }
        if (
            // createUserRequestDto.UserType == null - can this be optional in dto to get rid of error
            createUserRequestDto.Title == null
            || createUserRequestDto.FirstName == null
            || createUserRequestDto.LastName == null
            || createUserRequestDto.JobTitle == null
            || createUserRequestDto.WorkTelephone == null
            || createUserRequestDto.WorkEmail == null
        )
        {
            return Result<UserDetailsDto, CreateUserError>.Err(new CreateUserError.MissingFields());
        }
        bool UserExists = await dbContext.Users.AnyAsync(
            x => x.WorkEmail == createUserRequestDto.WorkEmail,
            cancellationToken: cancellationToken
        );
        if (UserExists)
        {
            return Result<UserDetailsDto, CreateUserError>.Err(new CreateUserError.EmailConflict());
        }
        var user = new User()
        {
            UserType = UserType.PharmaUser,
            Title = createUserRequestDto.Title,
            FirstName = createUserRequestDto.FirstName,
            LastName = createUserRequestDto.LastName,
            JobTitle = createUserRequestDto.JobTitle,
            WorkTelephone = createUserRequestDto.WorkTelephone,
            WorkEmail = createUserRequestDto.WorkEmail,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var userId = user.Id;
        _ = new UserOrgMembership()
        {
            UserId = userId,
            OrganisationId = createUserRequestDto.OrganisationId,
            UserRole = UserRole.Standard,
            Status = UserOrgStatus.RequestedAccess,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            // need to add created at
        };
        return Result<UserDetailsDto, CreateUserError>.Ok(MapToDto(user));
    }

    private static UserDetailsDto MapToDto(User user)
    {
        return new()
        {
            UserType = user.UserType,
            Title = user.Title,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JobTitle = user.JobTitle,
            WorkPhone = user.WorkTelephone,
            WorkEmail = user.WorkEmail,
        };
        // May need to be changed as currently can't have one user who is part of multiple organisations
    }
}
