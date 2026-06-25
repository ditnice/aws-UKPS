using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsOk_WhenOrganisationExists()
    {
        PaginatedResponseDto<UserListItemDto> expected = CreatePaginatedResponse();
        UserController controller = new(new StubUserService(expected));

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            CreateQuery()
        );

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        UserController controller = new(new StubUserService(null));

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            CreateQuery()
        );

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetUsers_ReturnsBadRequest_WhenQueryIsNull()
    {
        UserController controller = new(new StubUserService(CreatePaginatedResponse()));

        ActionResult<PaginatedResponseDto<UserListItemDto>> result = await controller.GetUsers(
            null
        );

        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public async Task GetUsers_PassesQueryValuesToService()
    {
        CapturingUserService service = new();
        UserController controller = new(service);
        GetUsersQueryDto query = new()
        {
            OrganisationId = 42,
            Page = 3,
            PageSize = 10,
            Status = [UserOrgStatus.Approved, UserOrgStatus.Inactive],
        };

        await controller.GetUsers(query);

        Assert.Equal(42, service.CapturedOrganisationId);
        Assert.Equal(3, service.CapturedPage);
        Assert.Equal(10, service.CapturedPageSize);
        Assert.Equal([UserOrgStatus.Approved, UserOrgStatus.Inactive], service.CapturedStatuses);
    }

    [Fact]
    public void GetUsersQueryDto_IsInvalid_WhenOrganisationIdIsMissing()
    {
        GetUsersQueryDto dto = new();

        List<ValidationResult> validationResults = Validate(dto);

        Assert.Contains(
            validationResults,
            r =>
                r.MemberNames.Contains(
                    nameof(GetUsersQueryDto.OrganisationId),
                    StringComparer.Ordinal
                )
        );
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
                    Status = UserOrgStatus.Approved,
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

    private sealed class StubUserService(PaginatedResponseDto<UserListItemDto>? result)
        : IUserService
    {
        public Task<PaginatedResponseDto<UserListItemDto>?> GetUsersByOrganisation(
            int organisationId,
            int page,
            int pageSize,
            IReadOnlyCollection<UserOrgStatus> statuses
        ) => Task.FromResult(result);
    }

    private sealed class CapturingUserService : IUserService
    {
        public int CapturedOrganisationId { get; private set; }
        public int CapturedPage { get; private set; }
        public int CapturedPageSize { get; private set; }
        public UserOrgStatus[] CapturedStatuses { get; private set; } = [];

        public Task<PaginatedResponseDto<UserListItemDto>?> GetUsersByOrganisation(
            int organisationId,
            int page,
            int pageSize,
            IReadOnlyCollection<UserOrgStatus> statuses
        )
        {
            CapturedOrganisationId = organisationId;
            CapturedPage = page;
            CapturedPageSize = pageSize;
            CapturedStatuses = statuses.ToArray();
            return Task.FromResult<PaginatedResponseDto<UserListItemDto>?>(
                CreatePaginatedResponse()
            );
        }
    }
}
