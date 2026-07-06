using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Controllers;

public class OrganisationControllerTests
{
    private static readonly DateTime _createdAt = new(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
    private static readonly DateTime _lastActive = new(2026, 6, 20, 12, 50, 1, DateTimeKind.Utc);

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
            Status = UserOrgStatus.Approved,
            LastActive = _lastActive,
            CreatedAt = _createdAt,
        };
        OrganisationController controller = new(new StubOrganisationService(getResult: expected));

        ActionResult<OrganisationDetailsDto> result = await controller.GetOrganisationById(1);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetOrganisationById_OrganisationDoesNotExist_ReturnsNotFound()
    {
        OrganisationController controller = new(new StubOrganisationService(getResult: null));

        ActionResult<OrganisationDetailsDto> result = await controller.GetOrganisationById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetOrganisationById_IdProvided_PassesIdToService()
    {
        CapturingOrganisationService service = new();
        OrganisationController controller = new(service);

        await controller.GetOrganisationById(42);

        Assert.Equal(42, service.CapturedId);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationExists_ReturnsOk()
    {
        OrganisationDetailsDto expected = CreateOrganisationDetailsDto();
        OrganisationController controller = new(
            new StubOrganisationService(updateResult: expected)
        );

        ActionResult<OrganisationDetailsDto> result = await controller.UpdateOrganisationDetails(
            1,
            CreateUpdateOrganisationDetailsDto()
        );

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_OrganisationDoesNotExist_ReturnsNotFound()
    {
        OrganisationController controller = new(new StubOrganisationService(updateResult: null));

        ActionResult<OrganisationDetailsDto> result = await controller.UpdateOrganisationDetails(
            99,
            CreateUpdateOrganisationDetailsDto()
        );

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_IdAndDtoProvided_PassesIdAndDtoToService()
    {
        CapturingOrganisationService service = new();
        OrganisationController controller = new(service);
        UpdateOrganisationDetailsDto request = CreateUpdateOrganisationDetailsDto();

        await controller.UpdateOrganisationDetails(42, request);

        Assert.Equal(42, service.CapturedUpdateId);
        Assert.Same(request, service.CapturedUpdateDto);
    }

    [Fact]
    public async Task UpdateOrganisationDetails_ModelStateIsInvalid_ReturnsBadRequest()
    {
        OrganisationController controller = new(new StubOrganisationService());
        controller.ModelState.AddModelError(
            nameof(UpdateOrganisationDetailsDto.OrganisationName),
            "Required"
        );

        ActionResult<OrganisationDetailsDto> result = await controller.UpdateOrganisationDetails(
            1,
            CreateUpdateOrganisationDetailsDto()
        );

        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        SerializableError errors = Assert.IsType<SerializableError>(badRequest.Value);
        string[] organisationNameErrors = Assert.IsType<string[]>(
            errors[nameof(UpdateOrganisationDetailsDto.OrganisationName)]
        );
        Assert.Contains("Required", organisationNameErrors);
    }

    [Fact]
    public void UpdateOrganisationDetailsDto_RequiredFieldsAreMissing_IsInvalid()
    {
        UpdateOrganisationDetailsDto dto = new();

        List<ValidationResult> validationResults = Validate(dto);

        string[] invalidMembers = validationResults
            .SelectMany(r => r.MemberNames)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.Contains(nameof(UpdateOrganisationDetailsDto.OrganisationName), invalidMembers);
        Assert.Contains(nameof(UpdateOrganisationDetailsDto.HeadOfficeAddress), invalidMembers);
        Assert.Contains(nameof(UpdateOrganisationDetailsDto.HeadOfficeEmail), invalidMembers);
        Assert.Contains(nameof(UpdateOrganisationDetailsDto.HeadOfficeTelephone), invalidMembers);
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

        Assert.Contains(
            validationResults,
            r =>
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

        Assert.DoesNotContain(
            validationResults,
            r =>
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

        Assert.Contains(
            validationResults,
            r =>
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
            Status = UserOrgStatus.Approved,
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
        List<ValidationResult> validationResults = [];
        Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            validateAllProperties: true
        );

        return validationResults;
    }

    private sealed class StubOrganisationService(
        OrganisationDetailsDto? getResult = null,
        OrganisationDetailsDto? updateResult = null
    ) : IOrganisationService
    {
        public Task<OrganisationDetailsDto?> GetOrganisationById(int id) =>
            Task.FromResult(getResult);

        public Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
            int id,
            UpdateOrganisationDetailsDto organisationDetails
        ) => Task.FromResult(updateResult);

        public Task<UserOrganisationMembershipDto?> UpdateUserOrganisationMembershipRole(
            int organisationId,
            int userId,
            UpdateUserOrganisationMembershipRoleCommandDto command,
            CancellationToken cancellationToken
        ) => Task.FromResult<UserOrganisationMembershipDto?>(null);
    }

    private sealed class CapturingOrganisationService : IOrganisationService
    {
        public int CapturedId { get; private set; }
        public int CapturedUpdateId { get; private set; }
        public UpdateOrganisationDetailsDto? CapturedUpdateDto { get; private set; }

        public Task<OrganisationDetailsDto?> GetOrganisationById(int id)
        {
            CapturedId = id;
            return Task.FromResult<OrganisationDetailsDto?>(null);
        }

        public Task<OrganisationDetailsDto?> UpdateOrganisationDetails(
            int id,
            UpdateOrganisationDetailsDto organisationDetails
        )
        {
            CapturedUpdateId = id;
            CapturedUpdateDto = organisationDetails;
            return Task.FromResult<OrganisationDetailsDto?>(null);
        }

        public Task<UserOrganisationMembershipDto?> UpdateUserOrganisationMembershipRole(
            int organisationId,
            int userId,
            UpdateUserOrganisationMembershipRoleCommandDto command,
            CancellationToken cancellationToken
        ) => Task.FromResult<UserOrganisationMembershipDto?>(null);
    }
}
