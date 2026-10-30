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

    [Theory]
    [InlineData(UserRole.Standard, OwnOrganisationId, true)]
    [InlineData(UserRole.Standard, OtherOrganisationId, false)]
    [InlineData(UserRole.Champion, OwnOrganisationId, true)]
    [InlineData(UserRole.Champion, OtherOrganisationId, false)]
    [InlineData(UserRole.Super, OwnOrganisationId, false)]
    [InlineData(UserRole.Super, OtherOrganisationId, false)]
    public void EditRecordContent_IsOnlyAllowedForStandardAndChampionUsersInTheirOrganisation(
        UserRole role,
        int organisationId,
        bool expected
    )
    {
        CanPerform(role, Operation.EditRecordContent, organisationId).ShouldBe(expected);
    }

    // Operation is internal, so the cases pass its value; xUnit test methods must be public.
    [Theory]
    [InlineData((int)Operation.Read)]
    [InlineData((int)Operation.Create)]
    [InlineData((int)Operation.Update)]
    public void SuperUser_KeepsEveryOtherOperationOnAnyOrganisation(int operation)
    {
        CanPerform(UserRole.Super, (Operation)operation, OtherOrganisationId).ShouldBeTrue();
    }

    private static bool CanPerform(UserRole role, Operation operation, int organisationId)
    {
        ICurrentUserInfoService currentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        currentUserInfoService
            .GetCurrentUserInfo()
            .Returns(
                AuthorisationTestConstants.DefaultCurrentUser with
                {
                    OrganisationId = OwnOrganisationId,
                    UserRole = role,
                }
            );
        return new OrganisationAuthoriser(currentUserInfoService)
            .GetAuthorisedOrganisations(operation)
            .Contains(organisationId);
    }
}
