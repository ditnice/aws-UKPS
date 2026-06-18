using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
using UKPS.Api.Enums;
using UKPS.Api.Services;

namespace UKPS.Api.Tests.Controllers;

public class OrganisationControllerTests
{
    [Fact]
    public async Task GetById_ReturnsOk_WhenOrganisationExists()
    {
        OrganisationDetailsDto expected = new()
        {
            Id = 1,
            OrganisationType = OrganisationType.PharmaCompany,
            OrganisationName = "Acme Pharma Ltd",
            HeadOfficeAddress = "1 High Street, London, EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
            HeadOfficeTelephone = "020 1234 5678"
        };
        OrganisationController controller = new(new StubOrganisationService(expected));

        IActionResult result = await controller.GetOrganisationById(1);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        OrganisationController controller = new(new StubOrganisationService(null));

        IActionResult result = await controller.GetOrganisationById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_PassesIdToService()
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
