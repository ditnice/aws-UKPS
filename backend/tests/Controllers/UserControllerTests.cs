using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using UKPS.Api.Common;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_mockUserService);
    }

    [Fact]
    public async Task GetUsers_ReturnsOk_WhenOrganisationExists()
    {
        PaginatedResponseDto<UserListItemDto> expected = CreatePaginatedResponse();
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(expected));

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await _controller.GetUsers(
            CreateQuery(),
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await _controller.GetUsers(
            CreateQuery(),
            TestContext.Current.CancellationToken
        );

        BadRequestObjectResult badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.Value.ShouldBe("Organisation not found.");
    }

    [Fact]
    public async Task GetUsers_ReturnsForbid_WhenNotAllowed()
    {
        var sampleId = 1;
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.NotAllowed(sampleId)
                )
            );

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await _controller.GetUsers(
            CreateQuery(),
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetUsers_ReturnsBadRequest_WhenQueryIsNull()
    {
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Ok(
                    CreatePaginatedResponse()
                )
            );

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await _controller.GetUsers(
            null,
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GetUsers_PassesQueryValuesToService()
    {
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );
        GetUsersQueryDto getUsersQuery = new()
        {
            OrganisationId = 42,
            Page = 3,
            PageSize = 10,
            Status = [UserOrgStatus.Active, UserOrgStatus.Inactive],
        };

        await _controller.GetUsers(getUsersQuery, TestContext.Current.CancellationToken);

        await _mockUserService
            .Received()
            .GetUsers(
                getUsersQuery.OrganisationId,
                getUsersQuery.Page,
                getUsersQuery.PageSize,
                Arg.Is<IReadOnlyCollection<UserOrgStatus>>(statuses =>
                    statuses.SequenceEqual(getUsersQuery.Status)
                ),
                TestContext.Current.CancellationToken
            );
    }

    [Fact]
    public async Task GetUsers_PassesNullOrganisationIdToService()
    {
        _mockUserService
            .GetUsers(
                Arg.Any<int?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );

        await _controller.GetUsers(new GetUsersQueryDto(), TestContext.Current.CancellationToken);

        await _mockUserService
            .Received(1)
            .GetUsers(
                null,
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<IReadOnlyCollection<UserOrgStatus>>(),
                TestContext.Current.CancellationToken
            );
    }

    [Fact]
    public void GetUsersQueryDto_IsValid_WhenOrganisationIdIsMissing()
    {
        GetUsersQueryDto dto = new();

        List<ValidationResult> validationResults = Validate(dto);

        validationResults.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetUsersQueryDto_IsInvalid_WhenPageIsLessThanOne(int page)
    {
        GetUsersQueryDto dto = new() { OrganisationId = 1, Page = page };

        List<ValidationResult> validationResults = Validate(dto);

        validationResults.ShouldContain(r =>
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

        validationResults.ShouldContain(r =>
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
