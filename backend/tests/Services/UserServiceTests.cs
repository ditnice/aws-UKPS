using Shouldly;
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

        result.IsErr.ShouldBeTrue();
        GetUsersError.OrganisationNotFound notFound =
            result.Error.ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(_organisationFaker.Generate());
        await Context.SaveChangesAsync();

        UserService service = new(Context);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(1, 1, 20, []);

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
        dto.Page.ShouldBe(1);
        dto.PageSize.ShouldBe(20);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;

        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(user.Id);
        item.EmailAddress.ShouldBe(user.WorkEmail);
        item.Role.ShouldBe(UserRole.Champion);
        item.Status.ShouldBe(UserOrgStatus.Active);
        item.LastActive.ShouldBeNull();
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([2, 3]);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(3);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 20]);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
        item.Status.ShouldBe(UserOrgStatus.Inactive);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(1);
        dto.Page.ShouldBe(5);
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

        result.IsOk.ShouldBeTrue();
        PaginatedResponseDto<UserListItemDto>? dto = result.Value;
        dto.ShouldNotBeNull();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 10]);
    }
}
