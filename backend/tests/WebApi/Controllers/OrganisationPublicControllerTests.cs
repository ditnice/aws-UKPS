using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.WebApi.Controllers;

namespace UKPS.Api.Tests.WebApi.Controllers;

public class OrganisationPublicControllerTests
{
    private readonly IOrganisationService _organisationServiceMock;
    private readonly OrganisationPublicController _controller;

    public OrganisationPublicControllerTests()
    {
        _organisationServiceMock = Substitute.For<IOrganisationService>();
        _controller = new OrganisationPublicController(_organisationServiceMock);
    }

    [Fact]
    public async Task GetAllOrganisations_OrganisationsExist_ReturnsOk()
    {
        IReadOnlyCollection<OrganisationListDto> expected =
        [
            new OrganisationListDto { Id = 1, OrganisationName = "Organisation1" },
            new OrganisationListDto { Id = 2, OrganisationName = "Organisation2" },
        ];

        _organisationServiceMock
            .GetAllOrganisations(TestContext.Current.CancellationToken)
            .Returns(expected);

        ActionResult<IReadOnlyCollection<OrganisationListDto>> result =
            await _controller.GetAllOrganisations(TestContext.Current.CancellationToken);

        OkObjectResult ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(expected);
    }
}
