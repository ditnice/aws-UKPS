using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.InternalServices.Authorisation;

public class OrganisationAuthoriserTests
{
    private const int OwnOrganisationId = 1;
    private const int OtherOrganisationId = 2;

    private static OrganisationAuthoriser CreateAuthoriser(UserRole role)
    {
        var currentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        currentUserInfoService
            .GetCurrentUserInfo()
            .Returns(
                AuthorisationTestConstants.DefaultCurrentUser with
                {
                    UserRole = role,
                    OrganisationId = OwnOrganisationId,
                }
            );
        return new OrganisationAuthoriser(currentUserInfoService);
    }

    [Theory]
    [InlineData(UserRole.Standard)]
    [InlineData(UserRole.Champion)]
    [InlineData(UserRole.Super)]
    public void EditRecordContent_IsAllowedOnOwnOrganisation(UserRole role)
    {
        var authoriser = CreateAuthoriser(role);

        authoriser
            .GetAuthorisedOrganisations(Operation.EditRecordContent)
            .Contains(OwnOrganisationId)
            .ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserRole.Standard, false)]
    [InlineData(UserRole.Champion, false)]
    [InlineData(UserRole.Super, true)]
    public void EditRecordContent_OnOtherOrganisation_IsOnlyAllowedForSuperUsers(
        UserRole role,
        bool expected
    )
    {
        var authoriser = CreateAuthoriser(role);

        authoriser
            .GetAuthorisedOrganisations(Operation.EditRecordContent)
            .Contains(OtherOrganisationId)
            .ShouldBe(expected);
    }

    [Fact]
    public void Update_IsStillNotAllowedForStandardUsers()
    {
        var authoriser = CreateAuthoriser(UserRole.Standard);

        authoriser
            .GetAuthorisedOrganisations(Operation.Update)
            .Contains(OwnOrganisationId)
            .ShouldBeFalse();
    }
}
