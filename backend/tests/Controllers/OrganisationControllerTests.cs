using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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

public class OrganisationControllerTests
{
    private static readonly DateTime _createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
    private static readonly DateTime _lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);
    private readonly IOrganisationService _organisationServiceMock;
    private readonly OrganisationController _controller;

    public OrganisationControllerTests()
    {
        _organisationServiceMock = Substitute.For<IOrganisationService>();

        _organisationServiceMock
            .GetOrganisationById(Arg.Any<int>())
            .Returns(callInfo =>
                Result<OrganisationDetailsDto, GetOrganisationByIdError>.Err(
                    new GetOrganisationByIdError.NotFound(callInfo.Arg<int>())
                )
            );

        _organisationServiceMock
            .UpdateOrganisationDetails(Arg.Any<int>(), Arg.Any<UpdateOrganisationDetailsDto>())
            .Returns(callInfo =>
                Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>.Err(
                    new UpdateOrganisationDetailsError.NotFound(callInfo.Arg<int>())
                )
            );

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
            .GetOrganisationById(1)
            .Returns(Result<OrganisationDetailsDto, GetOrganisationByIdError>.Ok(expected));

        ActionResult<OrganisationDetailsDto> result = await _controller.GetOrganisationById(1);

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFound()
    {
        ActionResult<OrganisationDetailsDto> result = await _controller.GetOrganisationById(99);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrganisationById_IdProvided_PassesIdToService()
    {
        var expectedId = 42;

        await _controller.GetOrganisationById(42);

        await _organisationServiceMock.Received(1).GetOrganisationById(expectedId);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_ReturnsOk()
    {
        OrganisationDetailsDto expected = CreateOrganisationDetailsDto();
        _organisationServiceMock
            .UpdateOrganisationDetails(Arg.Any<int>(), Arg.Any<UpdateOrganisationDetailsDto>())
            .Returns(Result<OrganisationDetailsDto, UpdateOrganisationDetailsError>.Ok(expected));

        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            1,
            CreateUpdateOrganisationDetailsDto()
        );

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFound()
    {
        ActionResult<OrganisationDetailsDto> result = await _controller.UpdateOrganisationDetails(
            99,
            CreateUpdateOrganisationDetailsDto()
        );

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateOrganisationDetails_IdAndDtoProvided_PassesIdAndDtoToService()
    {
        var exampleOrgId = 42;
        UpdateOrganisationDetailsDto request = CreateUpdateOrganisationDetailsDto();

        await _controller.UpdateOrganisationDetails(42, request);
        await _organisationServiceMock.Received(1).UpdateOrganisationDetails(exampleOrgId, request);
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
            CreateUpdateOrganisationDetailsDto()
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

        List<ValidationResult> validationResults = Validate(dto);

        validationResults.ShouldContain(r =>
            r.MemberNames.Contains(
                nameof(UpdateOrganisationDetailsDto.HeadOfficeEmail),
                StringComparer.Ordinal
            )
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
}
