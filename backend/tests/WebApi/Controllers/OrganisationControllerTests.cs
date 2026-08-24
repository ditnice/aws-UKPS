using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Organisations.Errors;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.WebApi.Controllers;
using UKPS.Api.WebApi.InternalServices.Authentication;
using DeactivateUserMembershipResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipDeactivateUserError
>;
using ReactivateUserMembershipResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipReactivateUserError
>;
using UpdateUserRoleResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Organisations.Dtos.OrganisationMembershipDto,
    UKPS.Api.Application.Organisations.Errors.OrganisationMembershipUpdateUserRoleError
>;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class OrganisationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateTime _createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
    private static readonly DateTime _lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
    private readonly IOrganisationService _organisationServiceMock;
    private readonly IOrganisationMembershipService _organisationMembershipService;
    private readonly HttpClient _client;
    private readonly OrganisationController _controller;

    public OrganisationControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _organisationMembershipService = Substitute.For<IOrganisationMembershipService>();
        _organisationServiceMock = Substitute.For<IOrganisationService>();
        _organisationServiceMock.Memberships.Returns(_organisationMembershipService);

        _organisationServiceMock
            .GetOrganisationById(Arg.Any<int>(), TestContext.Current.CancellationToken)
            .Returns(callInfo =>
                Result<OrganisationDetailsDto, GetOrganisationByIdError>.Err(
                    new GetOrganisationByIdError.NotFound(callInfo.Arg<int>())
                )
            );

        _organisationServiceMock
            .UpdateOrganisationDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateOrganisationDetailsDto>(),
                TestContext.Current.CancellationToken
            )
            .Returns(callInfo =>
                Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>.Err(
                    new UpdateOrganisationDetailsError.NotFound(callInfo.Arg<int>())
                )
            );

        _organisationMembershipService
            .ReactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                ReactivateUserMembershipResult.Err(
                    new OrganisationMembershipReactivateUserError.NotFound()
                )
            );

        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IOrganisationService>();
                    services.AddSingleton(_organisationServiceMock);
                });
                builder.ConfigureNoDatabase();
                builder.UseSetting("AWS:LoadSecrets", $"{false}");
                builder.UseSetting(
                    $"{DevAuthenticationConfiguration.SectionName}:{nameof(DevAuthenticationConfiguration.IsEnabled)}",
                    $"{true}"
                );
            })
            .CreateClient();

        _controller = new OrganisationController(_organisationServiceMock);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationExists_ReturnsOk()
    {
        OrganisationDetailsDto expected = new()
        {
            Id = 1,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
            Status = UserOrgStatus.Active,
            LastActive = _lastActive,
            CreatedAt = _createdAt,
        };
        _organisationServiceMock
            .GetOrganisationById(1, TestContext.Current.CancellationToken)
            .Returns(Result<OrganisationDetailsDto, GetOrganisationByIdError>.Ok(expected));

        ActionResult<OrganisationDetailsDto> result = await _controller.GetOrganisationById(
            1,
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFound()
    {
        ActionResult<OrganisationDetailsDto> result = await _controller.GetOrganisationById(
            99,
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrganisationById_ActionNotAllowed_ReturnsForbidden()
    {
        var sampleOrganisationId = 1;
        _organisationServiceMock
            .GetOrganisationById(sampleOrganisationId, TestContext.Current.CancellationToken)
            .Returns(
                Result<OrganisationDetailsDto, GetOrganisationByIdError>.Err(
                    new GetOrganisationByIdError.NotAllowed(sampleOrganisationId)
                )
            );
        ActionResult<OrganisationDetailsDto> result = await _controller.GetOrganisationById(
            1,
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetOrganisationById_IdProvided_PassesIdToService()
    {
        var expectedId = 42;

        await _controller.GetOrganisationById(42, TestContext.Current.CancellationToken);

        await _organisationServiceMock
            .Received(1)
            .GetOrganisationById(expectedId, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_ReturnsOk()
    {
        OrganisationDetailsDto expected = CreateOrganisationDetailsDto();
        _organisationServiceMock
            .UpdateOrganisationDetails(
                Arg.Any<int>(),
                Arg.Any<UpdateOrganisationDetailsDto>(),
                TestContext.Current.CancellationToken
            )
            .Returns(Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>.Ok(expected));

        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            1,
            CreateUpdateOrganisationDetailsDto(),
            TestContext.Current.CancellationToken
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task UpdateOrganisation_ActionNotAllowed_ReturnsForbidden()
    {
        var sampleOrganisationId = 1;
        _organisationServiceMock
            .UpdateOrganisationDetails(
                sampleOrganisationId,
                Arg.Any<UpdateOrganisationDetailsDto>(),
                TestContext.Current.CancellationToken
            )
            .Returns(
                Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>.Err(
                    new UpdateOrganisationDetailsError.NotAllowed(sampleOrganisationId)
                )
            );
        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            sampleOrganisationId,
            CreateUpdateOrganisationDetailsDto(),
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFound()
    {
        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            99,
            CreateUpdateOrganisationDetailsDto(),
            TestContext.Current.CancellationToken
        );

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateOrganisationDetails_IdAndDtoProvided_PassesIdAndDtoToService()
    {
        var exampleOrgId = 42;
        UpdateOrganisationDetailsDto request = CreateUpdateOrganisationDetailsDto();

        await _controller.UpdateOrganisationDetails(
            42,
            request,
            TestContext.Current.CancellationToken
        );
        await _organisationServiceMock
            .Received(1)
            .UpdateOrganisationDetails(
                exampleOrgId,
                request,
                TestContext.Current.CancellationToken
            );
    }

    [Fact]
    public async Task UpdateOrganisationDetails_ModelStateIsInvalid_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError(
            nameof(UpdateOrganisationDetailsDto.OrganisationName),
            "Required"
        );

        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            1,
            CreateUpdateOrganisationDetailsDto(),
            TestContext.Current.CancellationToken
        );

        BadRequestObjectResult badRequest = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        SerializableError errors = badRequest.Value.ShouldBeOfType<SerializableError>();
        string[] organisationNameErrors = errors[
            nameof(UpdateOrganisationDetailsDto.OrganisationName)
        ]
            .ShouldBeOfType<string[]>();
        organisationNameErrors.ShouldContain("Required");
    }

    [Fact]
    public void UpdateOrganisationDetailsDto_RequiredFieldsAreNull_IsInvalid()
    {
        UpdateOrganisationDetailsDto dto = JsonSerializer.Deserialize<UpdateOrganisationDetailsDto>(
            """
            {
                "OrganisationName": null,
                "HeadOfficeAddress": null,
                "HeadOfficeEmail": null,
                "HeadOfficeTelephone": null
            }
            """
        )!;

        List<ValidationResult> validationResults = Validate(dto);

        string[] invalidMembers = validationResults
            .SelectMany(r => r.MemberNames)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        invalidMembers.ShouldContain(nameof(UpdateOrganisationDetailsDto.OrganisationName));
        invalidMembers.ShouldContain(nameof(UpdateOrganisationDetailsDto.HeadOfficeAddress));
        invalidMembers.ShouldContain(nameof(UpdateOrganisationDetailsDto.HeadOfficeEmail));
        invalidMembers.ShouldContain(nameof(UpdateOrganisationDetailsDto.HeadOfficeTelephone));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    [InlineData(" \r\n ")]
    public void UpdateOrganisationDetailsDto_AddressIsWhitespace_IsInvalid(string address)
    {
        UpdateOrganisationDetailsDto dto = CreateUpdateOrganisationDetailsDto(address);

        List<ValidationResult> validationResults = Validate(dto);

        validationResults.ShouldContain(r =>
            r.MemberNames.Contains(
                nameof(UpdateOrganisationDetailsDto.HeadOfficeAddress),
                StringComparer.Ordinal
            )
            && string.Equals(
                r.ErrorMessage,
                "HeadOfficeAddress cannot be empty or whitespace.",
                StringComparison.Ordinal
            )
        );
    }

    [Fact]
    public void UpdateOrganisationDetailsDto_AddressIsMultiline_IsValid()
    {
        UpdateOrganisationDetailsDto dto = CreateUpdateOrganisationDetailsDto(
            "10 Downing Street\nLondon\nSW1A 2AA"
        );

        List<ValidationResult> validationResults = Validate(dto);

        validationResults.ShouldNotContain(r =>
            r.MemberNames.Contains(
                nameof(UpdateOrganisationDetailsDto.HeadOfficeAddress),
                StringComparer.Ordinal
            )
        );
    }

    [Fact]
    public void UpdateOrganisationDetailsDto_EmailIsInvalid_IsInvalid()
    {
        UpdateOrganisationDetailsDto dto = new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "not-an-email",
            HeadOfficeTelephone = "020 1234 5678",
        };

        List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = Validate(
            dto
        );

        validationResults.ShouldContain(r =>
            r.MemberNames.Contains(
                nameof(UpdateOrganisationDetailsDto.HeadOfficeEmail),
                StringComparer.Ordinal
            )
        );
    }

    [Fact]
    public async Task DeactivateMembership_UserIsNotAuthorised_ReturnsForbidResult()
    {
        _organisationMembershipService
            .DeactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                DeactivateUserMembershipResult.Err(
                    new OrganisationMembershipDeactivateUserError.NotAllowed(1)
                )
            );
        ActionResult<OrganisationMembershipDto> result = await _controller.DeactivateMembership(
            1,
            1,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<ObjectResult>()
            .StatusCode.ShouldBe((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateMembership_UserIsNotInAValidState_ReturnsBadRequestResult()
    {
        _organisationMembershipService
            .DeactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                DeactivateUserMembershipResult.Err(
                    new OrganisationMembershipDeactivateUserError.NotAllowedInCurrentState(
                        new StateMachineTransitionResult<UserOrgStatus>()
                        {
                            CurrentState = UserOrgStatus.Active,
                            Success = false,
                            PermittedNextState = [],
                        }
                    )
                )
            );
        ActionResult<OrganisationMembershipDto> result = await _controller.DeactivateMembership(
            1,
            1,
            TestContext.Current.CancellationToken
        );
        result
            .Result.ShouldBeOfType<ObjectResult>()
            .StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReactivateMembership_ShouldPassValuesToTheService()
    {
        var organisationId = 1;
        var membershipId = 2;
        _ = await RunReactivateRequest(membershipId, organisationId);

        await _organisationMembershipService
            .Received(1)
            .ReactivateMembership(organisationId, membershipId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateMembership_ShouldReturnMembershipDetails()
    {
        var expectedValue = new OrganisationMembershipDtoFaker().Generate();
        _organisationMembershipService
            .ReactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ReactivateUserMembershipResult.Ok(expectedValue));

        var response = await RunReactivateRequest();
        var data = await response.Content.ReadFromJsonAsync<OrganisationMembershipDto>(
            TestJsonOptions.Default,
            TestContext.Current.CancellationToken
        );

        data.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task ReactivateMembership_WhenNotAuthorised_ShouldReturnNotAuthorisedResult()
    {
        _organisationMembershipService
            .ReactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                ReactivateUserMembershipResult.Err(
                    new OrganisationMembershipReactivateUserError.NotAllowed()
                )
            );

        var response = await RunReactivateRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReactivateMembership_WhenMembershipNotFound_ShouldReturnNotFoundResult()
    {
        _organisationMembershipService
            .ReactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                ReactivateUserMembershipResult.Err(
                    new OrganisationMembershipReactivateUserError.NotFound()
                )
            );

        var response = await RunReactivateRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReactivateMembership_WhenNotAllowedInCurrentState_ShouldReturnBadRequest()
    {
        _organisationMembershipService
            .ReactivateMembership(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                ReactivateUserMembershipResult.Err(
                    new OrganisationMembershipReactivateUserError.NotAllowedInCurrentState(
                        new StateMachineTransitionResult<UserOrgStatus>()
                        {
                            Success = false,
                            CurrentState = UserOrgStatus.Active,
                            PermittedNextState = [],
                        }
                    )
                )
            );

        var response = await RunReactivateRequest();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserRole_UserIsNotAuthorised_ReturnsForbidResult()
    {
        _organisationMembershipService
            .UpdateUserRole(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<UpdateOrgMembershipUserRoleCommandDto>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                UpdateUserRoleResult.Err(
                    new OrganisationMembershipUpdateUserRoleError.NotAllowed(1)
                )
            );
        ActionResult<OrganisationMembershipDto> result = await _controller.UpdateUserRole(
            1,
            1,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );
        result.Result.ShouldBeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CreateOrganisation_AllFieldsProvided_ReturnsDto()
    {
        CreateOrganisationDto organisation = CreateOrganisationDto();
        OrganisationDetailsDto details = CreateOrganisationDetailsDto();
        _organisationServiceMock
            .CreateOrganisation(Arg.Any<CreateOrganisationDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<OrganisationDetailsDto, CreateOrganisationError>.Ok(details));

        await _controller.CreateOrganisation(organisation, TestContext.Current.CancellationToken);

        await _organisationServiceMock
            .Received(1)
            .CreateOrganisation(organisation, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateOrganisation_NameConflict_ReturnsConflict()
    {
        CreateOrganisationDto organisation = CreateOrganisationDto();
        _organisationServiceMock
            .CreateOrganisation(organisation, TestContext.Current.CancellationToken)
            .Returns(
                Result<OrganisationDetailsDto, CreateOrganisationError>.Err(
                    new CreateOrganisationError.OrganisationNameConflict()
                )
            );
        ActionResult<OrganisationDetailsDto> result = await _controller.CreateOrganisation(
            organisation,
            TestContext.Current.CancellationToken
        );
        await _organisationServiceMock
            .Received(1)
            .CreateOrganisation(organisation, TestContext.Current.CancellationToken);
        result.Result.ShouldBeOfType<ConflictObjectResult>();
    }

    private async Task<HttpResponseMessage> RunReactivateRequest(
        int membershipId = 1,
        int organisationId = 2
    )
    {
        using var emptyContent = new StringContent(string.Empty);
        return await _client.PatchAsync(
            new Uri(
                $"/organisations/{organisationId}/memberships/{membershipId}/reactivate",
                UriKind.Relative
            ),
            emptyContent,
            TestContext.Current.CancellationToken
        );
    }

    private static OrganisationDetailsDto CreateOrganisationDetailsDto() =>
        new()
        {
            Id = 1,
            OrganisationName = "Gov Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
            Status = UserOrgStatus.Active,
            LastActive = _lastActive,
            CreatedAt = _createdAt,
        };

    private static UpdateOrganisationDetailsDto CreateUpdateOrganisationDetailsDto() =>
        new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private static UpdateOrganisationDetailsDto CreateUpdateOrganisationDetailsDto(
        string address
    ) =>
        new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = address,
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private static List<ValidationResult> Validate(UpdateOrganisationDetailsDto dto)
    {
        List<ValidationResult> validationResults = new();
        Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true
        );
        return validationResults;
    }

    private static CreateOrganisationDto CreateOrganisationDto() =>
        new()
        {
            OrganisationName = "Gov Pharma Ltd",
            HeadOfficeAddress = "10 Downing Street\nLondon\nSW1A 2AA",
            HeadOfficeEmail = "info@pharma.gov.uk",
            HeadOfficeTelephone = "020 1234 5678",
        };

    private sealed class OrganisationMembershipDtoFaker : Faker<OrganisationMembershipDto>
    {
        public OrganisationMembershipDtoFaker()
        {
            RuleFor(x => x.Id, f => f.Random.Int(1));
            RuleFor(x => x.UserId, f => f.Random.Int(1));
            RuleFor(x => x.OrganisationId, f => f.Random.Int(1));

            RuleFor(x => x.UserRole, f => f.PickRandom<UserRole>());

            RuleFor(x => x.Status, f => f.PickRandom<UserOrgStatus>());

            RuleFor(x => x.AllowedPharmaceuticalEntity, f => f.PickRandom<PharmaceuticalEntity>());

            RuleFor(x => x.CreatedAt, f => f.Date.Recent());
        }
    }
}
