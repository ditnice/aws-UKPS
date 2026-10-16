using NSubstitute;
using Shouldly;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.InternalServices.Authorisation;

public class OrganisationAuthoriserTests
{
    private const int SelectedOrganisationId = 10;
    private const int OtherOrganisationId = 20;

    [Theory]
    [InlineData(nameof(Operation.Read))]
    [InlineData(nameof(Operation.Update))]
    [InlineData(nameof(Operation.SignUpUser))]
    public void CanPerformOperationOnOrganisation_WhenSuperUser_ShouldAllowAnyOrganisation(
        string operation
    )
    {
        var authoriser = CreateAuthoriser(UserRole.Super);

        authoriser
            .CanPerformOperationOnOrganisation(Parse(operation), OtherOrganisationId)
            .ShouldBeTrue();
    }

    [Theory]
    [InlineData(UserRole.Champion, nameof(Operation.Update), true)]
    [InlineData(UserRole.Standard, nameof(Operation.Read), true)]
    [InlineData(UserRole.Standard, nameof(Operation.Update), false)]
    public void CanPerformOperationOnOrganisation_WhenSelectedOrganisation_ShouldAllowOperationsForTheRole(
        UserRole role,
        string operation,
        bool expected
    )
    {
        var authoriser = CreateAuthoriser(
            role,
            new SelectableMembership(SelectedOrganisationId, role)
        );

        authoriser
            .CanPerformOperationOnOrganisation(Parse(operation), SelectedOrganisationId)
            .ShouldBe(expected);
    }

    [Fact]
    public void CanPerformOperationOnOrganisation_WhenNotAMemberOfTheOrganisation_ShouldNotAllow()
    {
        var authoriser = CreateAuthoriser(
            UserRole.Champion,
            new SelectableMembership(SelectedOrganisationId, UserRole.Champion)
        );

        authoriser
            .CanPerformOperationOnOrganisation(Operation.Read, OtherOrganisationId)
            .ShouldBeFalse();
    }

    [Theory]
    [InlineData(UserRole.Standard, nameof(Operation.Read))]
    [InlineData(UserRole.Champion, nameof(Operation.Update))]
    public void CanPerformOperationOnOrganisation_WhenPermittedInAnotherSelectableOrganisation_ShouldThrowMismatch(
        UserRole otherRole,
        string operation
    )
    {
        var authoriser = CreateAuthoriser(
            UserRole.Standard,
            new SelectableMembership(SelectedOrganisationId, UserRole.Standard),
            new SelectableMembership(OtherOrganisationId, otherRole)
        );

        SelectedOrganisationMismatchException exception =
            Should.Throw<SelectedOrganisationMismatchException>(() =>
                authoriser.CanPerformOperationOnOrganisation(Parse(operation), OtherOrganisationId)
            );
        exception.OrganisationId.ShouldBe(OtherOrganisationId);
    }

    [Fact]
    public void CanPerformOperationOnOrganisation_WhenRoleInAnotherSelectableOrganisationDoesNotPermitTheOperation_ShouldNotAllow()
    {
        var authoriser = CreateAuthoriser(
            UserRole.Champion,
            new SelectableMembership(SelectedOrganisationId, UserRole.Champion),
            new SelectableMembership(OtherOrganisationId, UserRole.Standard)
        );

        authoriser
            .CanPerformOperationOnOrganisation(Operation.Update, OtherOrganisationId)
            .ShouldBeFalse();
    }

    [Theory]
    [InlineData(UserRole.Champion, nameof(Operation.Update), false)]
    [InlineData(UserRole.Standard, nameof(Operation.Read), false)]
    [InlineData(UserRole.Standard, nameof(Operation.Update), true)]
    public void GetAuthorisedOrganisations_ShouldOnlyIncludeTheSelectedOrganisation(
        UserRole role,
        string operation,
        bool expectNone
    )
    {
        var authoriser = CreateAuthoriser(
            role,
            new SelectableMembership(SelectedOrganisationId, role),
            new SelectableMembership(OtherOrganisationId, role)
        );

        ValueOrAll<int> organisations = authoriser.GetAuthorisedOrganisations(Parse(operation));

        organisations.IsNone.ShouldBe(expectNone);
        organisations.Contains(SelectedOrganisationId).ShouldBe(!expectNone);
        organisations.Contains(OtherOrganisationId).ShouldBeFalse();
    }

    [Theory]
    [InlineData(SelectedOrganisationId, nameof(Operation.Read), "Allowed")]
    [InlineData(SelectedOrganisationId, nameof(Operation.Update), "NotAllowed")]
    [InlineData(OtherOrganisationId, nameof(Operation.Read), "NotSelected")]
    [InlineData(OtherOrganisationId, nameof(Operation.Update), "NotAllowed")]
    [InlineData(30, nameof(Operation.Read), "NotAllowed")]
    public void GetOrganisationAccess_ShouldNotThrowWhenTheOrganisationIsNotSelected(
        int organisationId,
        string operation,
        string expected
    )
    {
        var authoriser = CreateAuthoriser(
            UserRole.Standard,
            new SelectableMembership(SelectedOrganisationId, UserRole.Standard),
            new SelectableMembership(OtherOrganisationId, UserRole.Standard)
        );

        authoriser
            .GetOrganisationAccess(Parse(operation), organisationId)
            .ShouldBe(Enum.Parse<OrganisationAccess>(expected));
    }

    // Operation is internal, so theories pass its name.
    private static Operation Parse(string operation) => Enum.Parse<Operation>(operation);

    private static OrganisationAuthoriser CreateAuthoriser(
        UserRole role,
        params SelectableMembership[] selectableMemberships
    )
    {
        var currentUserInfoService = Substitute.For<ICurrentUserInfoService>();
        currentUserInfoService
            .GetCurrentUserInfo()
            .Returns(
                AuthorisationTestConstants.DefaultCurrentUser with
                {
                    OrganisationId = SelectedOrganisationId,
                    UserRole = role,
                    SelectableMemberships = selectableMemberships,
                }
            );

        return new OrganisationAuthoriser(currentUserInfoService);
    }
}
