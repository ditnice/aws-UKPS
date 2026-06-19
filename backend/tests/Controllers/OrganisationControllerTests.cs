using System.Data;
using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Tests.Controllers;

public class OrganisationControllerTests
{
    [Fact]
    public async Task GetOrganisationById_ReturnsOk_WhenOrganisationExists()
    {
        OrganisationDetailsDto expected = new()
        {
            Id = 1,
            OrganisationName = "Acme Pharma Ltd",
            OrganisationType = OrganisationType.PharmaCompany,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines,
            HeadOfficeAddress = "1 High Street, London, EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
            HeadOfficeTelephone = "020 1234 5678",
            Status = UserOrgStatus.Approved,
            LastActive = DateTime.Now,
            CreatedAt = DateTime.Now
        };
        OrganisationController controller = new(new StubOrganisationService(expected));

        IActionResult result = await controller.GetOrganisationById(1);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetOrganisationById_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        OrganisationController controller = new(new StubOrganisationService(null));

        IActionResult result = await controller.GetOrganisationById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetOrganisationById_PassesIdToService()
    {
        CapturingOrganisationService service = new();
        OrganisationController controller = new(service);

        await controller.GetOrganisationById(42);

        Assert.Equal(42, service.CapturedId);
    }

    private sealed class StubOrganisationService(OrganisationDetailsDto? result) : IOrganisationService
    {
        public Task<OrganisationDetailsDto?> GetOrganisationById(int id)
            => Task.FromResult(result);
    }

    private sealed class CapturingOrganisationService : IOrganisationService
    {
        public int CapturedId { get; private set; }

        public Task<OrganisationDetailsDto?> GetOrganisationById(int id)
        {
            CapturedId = id;
            return Task.FromResult<OrganisationDetailsDto?>(null);
        }
    }
}
