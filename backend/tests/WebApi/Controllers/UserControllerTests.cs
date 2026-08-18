using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.Controllers;

namespace UKPS.Api.Tests.WebApi.Controllers;

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
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
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
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
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
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
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
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
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
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
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
            Role = [UserRole.Champion, UserRole.Super],
            Email = "smith",
            LastActiveFrom = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            LastActiveTo = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
        };

        await _controller.GetUsers(getUsersQuery, TestContext.Current.CancellationToken);

        await _mockUserService
            .Received()
            .GetUsers(getUsersQuery, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetUsers_PassesNullOrganisationIdToService()
    {
        _mockUserService
            .GetUsers(Arg.Any<GetUsersQueryDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<PaginatedResponseDto<UserListItemDto>, GetUsersError>.Err(
                    new GetUsersError.OrganisationNotFound(1)
                )
            );

        await _controller.GetUsers(new GetUsersQueryDto(), TestContext.Current.CancellationToken);

        await _mockUserService
            .Received(1)
            .GetUsers(
                Arg.Is<GetUsersQueryDto>(query => query.OrganisationId == null),
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

    [Fact]
    public async Task CreateUser_IsValid_ReturnsDto()
    {
        CreateUserRequestDto request = CreateUserRequestDto();
        UserDetailsDto expected = UserDetailsDto();

        _mockUserService
            .CreateUser(request, TestContext.Current.CancellationToken)
            .Returns(Result<UserDetailsDto, CreateUserError>.Ok(expected));

        ActionResult<UserDetailsDto> result = await _controller.CreateUser(
            request,
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task CreateUser_OrgNotFound_ReturnsNotFound()
    {
        CreateUserRequestDto request = CreateUserRequestDto();
        _mockUserService
            .CreateUser(Arg.Any<CreateUserRequestDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<UserDetailsDto, CreateUserError>.Err(
                    new CreateUserError.NotFound(request.OrganisationId)
                )
            );
        ActionResult<UserDetailsDto> result = await _controller.CreateUser(
            request,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<NotFoundObjectResult>()
            .Value.ShouldBe("There is no organisation with that Organisation ID.");
    }

    [Fact]
    public async Task CreateUser_EmailConflict_ReturnsConflict()
    {
        CreateUserRequestDto request = CreateUserRequestDto();
        _mockUserService
            .CreateUser(Arg.Any<CreateUserRequestDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<UserDetailsDto, CreateUserError>.Err(new CreateUserError.EmailConflict())
            );
        ActionResult<UserDetailsDto> result = await _controller.CreateUser(
            request,
            TestContext.Current.CancellationToken
        );
        ConflictObjectResult conflict = result.Result.ShouldBeOfType<ConflictObjectResult>();
        conflict.Value.ShouldBe("A user with that email is already registered.");
    }

    [Fact]
    public async Task CreateUser_FieldsMissing_ReturnsBadRequest()
    {
        CreateUserRequestDto request = CreateUserRequestDto();

        _mockUserService
            .CreateUser(Arg.Any<CreateUserRequestDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<UserDetailsDto, CreateUserError>.Err(new CreateUserError.MissingFields())
            );
        ActionResult<UserDetailsDto> result = await _controller.CreateUser(
            request,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<BadRequestObjectResult>()
            .Value.ShouldBe("Some of the data required is missing.");
    }

    [Fact]
    public async Task RegisterUser_IsValid_ReturnsDto()
    {
        RegisterUserDto request = RegisterUserDto();
        RegisterUserDetailsDto expected = RegisterUserDetailsDto();

        _mockUserService
            .RegisterUser(request, TestContext.Current.CancellationToken)
            .Returns(Result<RegisterUserDetailsDto, RegisterUserError>.Ok(expected));

        ActionResult<RegisterUserDetailsDto> result = await _controller.RegisterUser(
            request,
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task RegisterUser_FieldsMissing_ReturnsBadRequest()
    {
        RegisterUserDto request = RegisterUserDto();
        _mockUserService
            .RegisterUser(Arg.Any<RegisterUserDto>(), TestContext.Current.CancellationToken)
            .Returns(
                Result<RegisterUserDetailsDto, RegisterUserError>.Err(
                    new RegisterUserError.MissingFields()
                )
            );
        ActionResult<RegisterUserDetailsDto> result = await _controller.RegisterUser(
            request,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<BadRequestObjectResult>()
            .Value.ShouldBe("Some of the data required is missing.");
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

    private static UserDetailsDto UserDetailsDto() =>
        new()
        {
            UserType = UserType.PharmaUser,
            Title = "Mr",
            FullName = "Test1",
            JobTitle = "Test3",
            WorkPhone = "0123456789",
            WorkEmail = "user@example.com",
        };

    private static CreateUserRequestDto CreateUserRequestDto() =>
        new()
        {
            UserType = UserType.PharmaUser,
            Title = "Mr",
            FullName = "Test1",
            JobTitle = "Test3",
            WorkTelephone = "0123456789",
            WorkEmail = "user@example.com",
            OrganisationId = 1,
        };

    private static RegisterUserDto RegisterUserDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "0123456789",
            WorkEmail = "user@example.com",
            Organisation = "Test2",
        };

    private static RegisterUserDetailsDto RegisterUserDetailsDto() =>
        new()
        {
            FullName = "Test1",
            PhoneNumber = "0123456789",
            WorkEmail = "user@example.com",
        };
}
