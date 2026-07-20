using Microsoft.EntityFrameworkCore;
using UKPS.Api.Common;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using GetUsersResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.PaginatedResponseDto<UKPS.Api.DTOs.UserListItemDto>,
    UKPS.Api.Services.Errors.GetUsersError
>;

namespace UKPS.Api.Services;

internal sealed class UserService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IUserService
{
    public async Task<GetUsersResult> GetUsers(
        int? organisationId,
        int page,
        int pageSize,
        IReadOnlyCollection<UserOrgStatus> statuses
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
            bool organisationExists = await dbContext.Organisations.AnyAsync(o =>
                o.Id == organisationId.Value
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

        int totalCount = await organisationMemberships.CountAsync();

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
            .ToListAsync();

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
        CreateUserRequestDto createUserRequestDto
    )
    {
        var organisation = await dbContext.Organisations.FindAsync(
            createUserRequestDto.OrganisationId
        );

        if (organisation is null)
        {
            return Result<UserDetailsDto, CreateUserError>.Err(
                new CreateUserError.NotFound(createUserRequestDto.OrganisationId)
            );
        }
        var user = new User()
        {
            Username = createUserRequestDto.Username,
            UserType = UserType.PharmaUser,
            Title = createUserRequestDto.Title,
            FirstName = createUserRequestDto.FirstName,
            LastName = createUserRequestDto.LastName,
            JobTitle = createUserRequestDto.JobTitle,
            WorkTelephone = createUserRequestDto.WorkTelephone,
            WorkEmail = createUserRequestDto.WorkEmail,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userId = user.Id;
        _ = new UserOrgMembership()
        {
            UserId = userId,
            OrganisationId = createUserRequestDto.OrganisationId,
            UserRole = UserRole.Standard,
            Status = UserOrgStatus.RequestedAccess,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
        };
        return Result<UserDetailsDto, CreateUserError>.Ok(MapToDto(user));
    }

    private static UserDetailsDto MapToDto(User user)
    {
        return new()
        {
            Username = user.Username,
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
