using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.InternalServices.Identity;

public class CurrentUserInfoServiceExtensionsTests
{
    private const int SelectedOrganisationId = 10;
    private const int OtherOrganisationId = 20;
    private const int NonMemberOrganisationId = 30;

    private readonly ICurrentUserInfoService _currentUserInfoService =
        Substitute.For<ICurrentUserInfoService>();

    public CurrentUserInfoServiceExtensionsTests()
    {
        _currentUserInfoService
            .GetCurrentUserInfo()
            .Returns(
                AuthorisationTestConstants.DefaultCurrentUser with
                {
                    OrganisationId = SelectedOrganisationId,
                    SelectableOrganisationIds = [SelectedOrganisationId, OtherOrganisationId],
                }
            );
    }

    [Fact]
    public void ThrowIfOrganisationNotSelected_WhenSelectedOrganisation_ShouldNotThrow()
    {
        Should.NotThrow(() =>
            _currentUserInfoService.ThrowIfOrganisationNotSelected(SelectedOrganisationId)
        );
    }

    [Fact]
    public void ThrowIfOrganisationNotSelected_WhenMemberButNotSelected_ShouldThrowMismatch()
    {
        SelectedOrganisationMismatchException exception =
            Should.Throw<SelectedOrganisationMismatchException>(() =>
                _currentUserInfoService.ThrowIfOrganisationNotSelected(OtherOrganisationId)
            );

        exception.OrganisationId.ShouldBe(OtherOrganisationId);
    }

    [Fact]
    public void ThrowIfOrganisationNotSelected_WhenNotAMember_ShouldNotThrow()
    {
        Should.NotThrow(() =>
            _currentUserInfoService.ThrowIfOrganisationNotSelected(NonMemberOrganisationId)
        );
    }
}
