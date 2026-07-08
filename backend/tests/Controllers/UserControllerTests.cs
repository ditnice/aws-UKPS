using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UKPS.Api.Common;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsOk_WhenOrganisationExists()
    {
        PaginatedResponseDto<UserListItemDto> expected = CreatePaginatedResponse();
        var mockUserService = Substitute.For<IUserService>();
        mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            )
            .Returns(Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(expected));
        UserController controller = new(mockUserService);

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            CreateQuery()
        );

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );
        UserController controller = new(mockUserService);

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            CreateQuery()
        );

        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Organisation not found.", badRequest.Value);
    }

    [Fact]
    public async Task GetUsers_ReturnsBadRequest_WhenQueryIsNull()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(
                    CreatePaginatedResponse()
                )
            );
        UserController controller = new(mockUserService);

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            null
        );

        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public async Task GetUsers_PassesQueryValuesToService()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );
        UserController controller = new(mockUserService);
        GetUsersQueryDto getUsersQuery = new()
        {
            OrganisationId = 42,
            Page = 3,
            PageSize = 10,
            Status = [UserOrgStatus.Active, UserOrgStatus.Inactive],
        };

        await controller.GetUsers(getUsersQuery);

        await mockUserService
            .Received()
            .GetUsers(
                getUsersQuery.OrganisationId,
                getUsersQuery.Page,
                getUsersQuery.PageSize,
                Arg.Is<IReadOnlyCollection<UserOrgStatus>>(statuses =>
                    statuses.SequenceEqual(getUsersQuery.Status)
                )
            );
    }

    [Fact]
    public async Task GetUsers_PassesNullOrganisationIdToService()
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );
        UserController controller = new(mockUserService);

        await controller.GetUsers(new GetUsersQueryDto());

        await mockUserService
            .Received(1)
            .GetUsers(
                null,
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>()
            );
    }

    [Fact]
    public void GetUsersQueryDto_IsValid_WhenOrganisationIdIsMissing()
    {
        GetUsersQueryDto dto = new();

        List<ValidationResult> validationResults = Validate(dto);

        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetUsersQueryDto_IsInvalid_WhenPageIsLessThanOne(int page)
    {
        GetUsersQueryDto dto = new() { OrganisationId = 1, Page = page };

        List<ValidationResult> validationResults = Validate(dto);

        Assert.Contains(
            validationResults,
            r =>
                r.MemberNames.Contains(nameof(GetUsersQueryDto.Page), StringComparer.Ordinal)
                && string.Equals(
                    r.ErrorMessage,
                    "Page cannot be less than 1.",
                    StringComparison.Ordinal
                )
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void GetUsersQueryDto_IsInvalid_WhenPageSizeIsOutsideAllowedRange(int pageSize)
    {
        GetUsersQueryDto dto = new() { OrganisationId = 1, PageSize = pageSize };

        List<ValidationResult> validationResults = Validate(dto);

        Assert.Contains(
            validationResults,
            r =>
                r.MemberNames.Contains(nameof(GetUsersQueryDto.PageSize), StringComparer.Ordinal)
                && string.Equals(
                    r.ErrorMessage,
                    "PageSize must be between 1 and 100.",
                    StringComparison.Ordinal
                )
        );
    }

    private static GetUsersQueryDto CreateQuery() =>
        new()
        {
            OrganisationId = 1,
            Page = 1,
            PageSize = 20,
        };

    private static PaginatedResponseDto<UserListItemDto> CreatePaginatedResponse() =>
        new()
        {
            Items =
            [
                new UserListItemDto
                {
                    UserId = 1,
                    EmailAddress = "user@example.com",
                    Role = UserRole.Standard,
                    Status = UserOrgStatus.Active,
                },
            ],
            TotalCount = 1,
            Page = 1,
            PageSize = 20,
        };

    private static List<ValidationResult> Validate(GetUsersQueryDto dto)
    {
        List<ValidationResult> validationResults = [];
        Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true
        );

        return validationResults;
    }
}
