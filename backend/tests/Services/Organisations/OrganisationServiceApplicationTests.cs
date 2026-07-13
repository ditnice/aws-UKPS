using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.GetOrganisationByIdError
>;
using UpdateOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.UpdateOrganisationDetailsError
>;

namespace UKPS.Api.Tests.Services.Organisations;

[Collection(DatabaseCollection.Name)]
public class OrganisationServiceApplicationTests(PostgresFixture fixture)
    : OrganisationServiceTests(fixture)
{
    internal override IServiceTestHarness<IOrganisationService> ServiceTestHarness =>
        new ServiceTestHarness<IOrganisationService>(Context);

    // Move authorisation test to based class once this has been implemented in the presentation layer.

    [Theory]
    [InlineData(true, UserRole.Super, true)]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(true, UserRole.Standard, true)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task GetOrganisationById_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        Organisation organisation = await AddEntity(_organisationFaker.Generate());
        int otherOrganisationId = 999_999;
        int usersOrganisationId = organisationIdMatches ? organisation.Id : otherOrganisationId;

        var testHarness = ServiceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = usersOrganisationId,
                UserRole = userRole,
            }
        );
        var service = testHarness.Service;

        GetOrganisationResult result = await service.GetOrganisationById(organisation.Id);

        if (expectedAuthorised)
        {
            result.IsOk.ShouldBeTrue();
        }
        else
        {
            result.IsErr.ShouldBeTrue();
            result.Error.ShouldBeOfType<GetOrganisationByIdError.NotAllowed>();
        }
    }

    [Theory]
    [InlineData(true, UserRole.Super, true)]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(true, UserRole.Standard, false)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task UpdateOrganisationDetails_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        Organisation organisation = await AddEntity(_organisationFaker.Generate());
        int otherOrganisationId = 999_999;
        int usersOrganisationId = organisationIdMatches ? organisation.Id : otherOrganisationId;

        var testHarness = ServiceTestHarness.UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = usersOrganisationId,
                UserRole = userRole,
            }
        );
        var service = testHarness.Service;

        var updateCommand = new UpdateOrganisationDetailsDtoFaker().Generate();
        UpdateOrganisationResult result = await service.UpdateOrganisationDetails(
            organisation.Id,
            updateCommand
        );

        if (expectedAuthorised)
        {
            result.IsOk.ShouldBeTrue();
        }
        else
        {
            result.IsErr.ShouldBeTrue();
            result.Error.ShouldBeOfType<UpdateOrganisationDetailsError.NotAllowed>();
        }
    }
}
