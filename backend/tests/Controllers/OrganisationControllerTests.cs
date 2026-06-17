using Microsoft.AspNetCore.Mvc;
using UKPS.Api.Controllers;
using UKPS.Api.DTOs;
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
            OrganisationType = "PharmaCompany",
            OrganisationName = "Acme Pharma Ltd",
            HeadOfficeAddress = "1 High Street, London, EC1A 1AA",
            HeadOfficeEmail = "info@acme.com",
            HeadOfficeTelephone = "020 1234 5678"
        };
        OrganisationController controller = new(new StubOrganisationService(expected));

        IActionResult result = await controller.GetByIdAsync(1, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOrganisationDoesNotExist()
    {
        OrganisationController controller = new(new StubOrganisationService(null));

        IActionResult result = await controller.GetByIdAsync(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_PassesIdToService()
    {
        CapturingOrganisationService service = new();
        OrganisationController controller = new(service);

        await controller.GetByIdAsync(42, CancellationToken.None);

        Assert.Equal(42, service.CapturedId);
    }

    private sealed class StubOrganisationService(OrganisationDetailsDto? result) : IOrganisationService
    {
        public Task<OrganisationDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    private sealed class CapturingOrganisationService : IOrganisationService
    {
        public int CapturedId { get; private set; }

        public Task<OrganisationDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            CapturedId = id;
            return Task.FromResult<OrganisationDetailsDto?>(null);
        }
    }
}
