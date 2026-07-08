using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(99, 1, 20, []);

        Assert.True(result.IsErr);
        GetUsersError.OrganisationNotFound notFound =
            Assert.IsType<GetUsersError.OrganisationNotFound>(result.Error);
        Assert.Equal(99, notFound.OrganisationId);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(_organisationFaker.Generate());
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Empty(dto.Items);
        Assert.Equal(0, dto.TotalCount);
        Assert.Equal(1, dto.Page);
        Assert.Equal(20, dto.PageSize);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        var organisation = _organisationFaker.Generate();
        var user = _userFaker.Generate() with { WorkEmail = "user@example.com" };
        var membership = _userOrgMembershipFaker.Generate() with
        {
            UserId = user.Id,
            OrganisationId = organisation.Id,
            UserRole = UserRole.Champion,
            Status = UserOrgStatus.Active,
        };
        Context.Organisations.Add(organisation);
        Context.Users.Add(user);
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(user.Id, item.UserId);
        Assert.Equal(user.WorkEmail, item.EmailAddress);
        Assert.Equal(UserRole.Champion, item.Role);
        Assert.Equal(UserOrgStatus.Active, item.Status);
        Assert.Null(item.LastActive);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        Context.Organisations.Add(_organisationFaker.Generate() with { Id = 1 });
        Context.Users.AddRange(
            _userFaker.Generate() with
            {
                Id = 1,
                WorkEmail = "requested-access@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 2,
                WorkEmail = "active@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 3,
                WorkEmail = "inactive@example.com",
            }
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 1,
                OrganisationId = 1,
                Status = UserOrgStatus.RequestedAccess,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 2,
                UserId = 2,
                OrganisationId = 1,
                Status = UserOrgStatus.Active,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 3,
                UserId = 3,
                OrganisationId = 1,
                Status = UserOrgStatus.Inactive,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, [UserOrgStatus.Active, UserOrgStatus.Inactive]);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([2, 3], dto.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_PaginatesAndOrdersByUserId_WhenUsersExist()
    {
        Context.Organisations.Add(_organisationFaker.Generate() with { Id = 1 });
        Context.Users.AddRange(
            _userFaker.Generate() with
            {
                Id = 30,
                WorkEmail = "thirty@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 10,
                WorkEmail = "ten@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 20,
                WorkEmail = "twenty@example.com",
            }
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 30,
                OrganisationId = 1,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 2,
                UserId = 10,
                OrganisationId = 1,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 3,
                UserId = 20,
                OrganisationId = 1,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 2, 1, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(3, dto.TotalCount);
        Assert.Equal(2, dto.Page);
        Assert.Equal(1, dto.PageSize);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(20, item.UserId);
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate() with
            {
                Id = 1,
            },
            _organisationFaker.Generate() with
            {
                Id = 2,
            }
        );
        Context.Users.AddRange(
            _userFaker.Generate() with
            {
                Id = 10,
                WorkEmail = "one@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 20,
                WorkEmail = "two@example.com",
            }
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 10,
                OrganisationId = 1,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 2,
                UserId = 20,
                OrganisationId = 2,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([10, 20], dto.Items.Select(i => i.UserId).ToArray());
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate() with
            {
                Id = 1,
            },
            _organisationFaker.Generate() with
            {
                Id = 2,
            }
        );
        Context.Users.AddRange(
            _userFaker.Generate() with
            {
                Id = 10,
                WorkEmail = "active@example.com",
            },
            _userFaker.Generate() with
            {
                Id = 20,
                WorkEmail = "inactive@example.com",
            }
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 10,
                OrganisationId = 1,
                Status = UserOrgStatus.Active,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 2,
                UserId = 20,
                OrganisationId = 2,
                Status = UserOrgStatus.Inactive,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, [UserOrgStatus.Inactive]);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(1, dto.TotalCount);
        UserListItemDto item = Assert.Single(dto.Items);
        Assert.Equal(20, item.UserId);
        Assert.Equal(UserOrgStatus.Inactive, item.Status);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        Context.Organisations.Add(_organisationFaker.Generate() with { Id = 1 });
        Context.Users.Add(_userFaker.Generate() with { Id = 10, WorkEmail = "user@example.com" });
        Context.UserOrgMemberships.Add(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 10,
                OrganisationId = 1,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 5, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Empty(dto.Items);
        Assert.Equal(1, dto.TotalCount);
        Assert.Equal(5, dto.Page);
    }

    [Fact]
    public async Task GetUsers_UserHasMembershipsInMultipleOrganisations_ReturnsOneRowPerMembership()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate() with
            {
                Id = 1,
            },
            _organisationFaker.Generate() with
            {
                Id = 2,
            }
        );
        Context.Users.Add(_userFaker.Generate() with { Id = 10, WorkEmail = "multi@example.com" });
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker.Generate() with
            {
                Id = 1,
                UserId = 10,
                OrganisationId = 1,
                AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            },
            _userOrgMembershipFaker.Generate() with
            {
                Id = 2,
                UserId = 10,
                OrganisationId = 2,
                AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            }
        );
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(null, 1, 20, []);

        Assert.True(result.IsOk);
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(2, dto.TotalCount);
        Assert.Equal([10, 10], dto.Items.Select(i => i.UserId).ToArray());
    }
}
