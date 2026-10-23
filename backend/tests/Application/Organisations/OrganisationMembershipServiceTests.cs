using Bogus;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Organisations;
using UKPS.Api.Application.Organisations.Dtos;
using UKPS.Api.Application.Organisations.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.Organisations;

[Collection(DatabaseCollection.Name)]
public class OrganisationMembershipServiceTests : DatabaseTestBase
{
    private readonly UserFaker _userFaker;
    private readonly UserOrgMembershipFaker _membershipFaker;
    private readonly OrganisationFaker _organisationFaker;
    private readonly ServiceTestHarness<IOrganisationMembershipService> _harness;
    private readonly IOrganisationMembershipService _service;

    public OrganisationMembershipServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _userFaker = new UserFaker();
        _membershipFaker = new UserOrgMembershipFaker();
        _membershipFaker.RuleFor(x => x.UserRole, _ => UserRole.Standard);
        _membershipFaker.RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active);
        _organisationFaker = new OrganisationFaker();
        _harness = new ServiceTestHarness<IOrganisationMembershipService>(Context);
        _service = _harness.Service;
    }

    [Theory]
    [InlineData(UserRole.Champion)]
    [InlineData(UserRole.Super)]
    public async Task UpdateUserOrganisationMembershipRole_ShouldUpdatedUserMembershipRole(
        UserRole userRole
    )
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var command = new UpdateOrgMembershipUserRoleCommandDto() { UserRole = userRole };
        var result = await _service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            command,
            CancellationToken.None
        );

        var dto = result.ShouldBeSuccess();
        dto.UserRole.ShouldBe(userRole);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(userRole);
    }

    [Theory]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(true, UserRole.Standard, false)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task UpdateUserRole_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.UserRole,
                _ => UserRole.Standard
            )
        );
        var command = new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Champion };
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = organisationIdMatches ? userOrgMembership.OrganisationId : 999_999,
                UserRole = userRole,
            }
        );
        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            command,
            CancellationToken.None
        );

        if (expectedAuthorised)
        {
            result.ShouldBeSuccess();
            return;
        }

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotAllowed>();
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnError_WhenNonSuperUserTriesToPromoteToSuper()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.UserRole,
                _ => UserRole.Standard
            )
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = userOrgMembership.OrganisationId,
                UserRole = UserRole.Champion,
            }
        );

        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Super },
            CancellationToken.None
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.CannotManageSuperRole>();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(UserRole.Standard);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnError_WhenNonSuperUserTriesToChangeASuperUsersRole()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.UserRole, _ => UserRole.Super)
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = userOrgMembership.OrganisationId,
                UserRole = UserRole.Champion,
            }
        );

        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            CancellationToken.None
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.CannotManageSuperRole>();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(UserRole.Super);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldSucceed_WhenSuperUserPromotesAnotherUserToSuper()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.UserRole,
                _ => UserRole.Standard
            )
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = userOrgMembership.OrganisationId,
                UserRole = UserRole.Super,
            }
        );

        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Super },
            CancellationToken.None
        );

        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Super);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldSucceed_WhenSuperUserChangesAnotherSuperUsersRole()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.UserRole, _ => UserRole.Super)
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = userOrgMembership.OrganisationId,
                UserRole = UserRole.Super,
            }
        );

        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            CancellationToken.None
        );

        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);
    }

    [Fact]
    public async Task UpdateUserOrganisationMembershipRole_OrganisationDoesNotExist_ShouldReturnNull()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var command = new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Champion };
        var result = await _service.UpdateUserRole(
            999_999,
            userOrgMembership.Id,
            command,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotFound>();
    }

    [Fact]
    public async Task UpdateUserOrganisationMembershipRole_MembershipDoesNotExist_ShouldReturnNull()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var command = new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Champion };
        var result = await _service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            999_999,
            command,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotFound>();
    }

    [Fact]
    public async Task DeactivateMembership_ShouldDeactivateTheSpecifiedMembership()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var dto = result.ShouldBeSuccess();
        dto.Status.ShouldBe(UserOrgStatus.Deactivated);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.Status.ShouldBe(UserOrgMembershipStatus.Deactivated);
    }

    [Fact]
    public async Task DeactivateMembership_WhenAUserIsDeactivatedTwice_AUserShouldOnlyGetOneNotificationEmail()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );

        foreach (var _ in Enumerable.Range(0, 2))
        {
            var result = await _service.DeactivateMembership(
                userOrgMembership.OrganisationId,
                userOrgMembership.Id,
                CancellationToken.None
            );

            result.ShouldBeSuccess();
        }

        _harness.Emails.Sent.Count.ShouldBe(1);
    }

    [Fact]
    public async Task DeactivateMembership_ShouldSendNotificationEmail()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.ShouldBeSuccess();

        _harness.Emails.Sent.Single().ShouldBeOfType<DeactivatedUserNotificationEmail>();
    }

    [Fact]
    public async Task DeactivateMembership_WhenInInvalidInitialState_ShouldReturnError()
    {
        var invalidInitialState = UserOrgMembershipStatus.AwaitingSetup;
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => invalidInitialState
            )
        );
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var error = result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotAllowedInCurrentState>();
        error.TransitionResult.CurrentState.ShouldBe(invalidInitialState.ConvertToUserOrgStatus());
    }

    [Theory]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(true, UserRole.Standard, false)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task DeactivateMembership_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = organisationIdMatches ? userOrgMembership.OrganisationId : 999_999,
                UserRole = userRole,
            }
        );

        Result<OrganisationMembershipDto, OrganisationMembershipDeactivateUserError> result =
            await harness.Service.DeactivateMembership(
                userOrgMembership.OrganisationId,
                userOrgMembership.Id,
                CancellationToken.None
            );

        if (expectedAuthorised)
        {
            result.ShouldBeSuccess();
            return;
        }

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotAllowed>();
    }

    [Fact]
    public async Task DeactivateMembership_MembershipAlreadyDeactivated_ReturnsOkIdempotently()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Deactivated
            )
        );
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var dto = result.ShouldBeSuccess();
        dto.Status.ShouldBe(UserOrgStatus.Deactivated);
    }

    [Fact]
    public async Task DeactivateMembership_WhenOrganisationDoesNotExist_ShouldReturnNotFoundResult()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var result = await _service.DeactivateMembership(
            999_999,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotFound>();
    }

    [Fact]
    public async Task DeactivateMembership_WhenMembershipDoesNotExist_ShouldReturnNotFoundResult()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            999_999,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotFound>();
    }

    [Fact]
    public async Task ReactivateMembership_WhenAUserIsReactivatedTwice_AUserShouldOnlyGetOneNotificationEmail()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Deactivated
            )
        );

        foreach (var _ in Enumerable.Range(0, 2))
        {
            var result = await _service.ReactivateMembership(
                userOrgMembership.OrganisationId,
                userOrgMembership.Id,
                CancellationToken.None
            );

            result.ShouldBeSuccess();
        }

        _harness.Emails.Sent.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ReactivateMembership_ShouldReactivateTheSpecifiedMembership()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Deactivated
            )
        );
        var result = await _service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var dto = result.ShouldBeSuccess();
        dto.Status.ShouldBe(UserOrgStatus.Active);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.Status.ShouldBe(UserOrgMembershipStatus.Active);
    }

    [Fact]
    public async Task ReactivateMembership_WhenInInvalidInitialState_ShouldReturnError()
    {
        var invalidInitialState = UserOrgMembershipStatus.AwaitingSetup;
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => invalidInitialState
            )
        );
        var result = await _service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var error = result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipReactivateUserError.NotAllowedInCurrentState>();
        error.TransitionResult.CurrentState.ShouldBe(
            invalidInitialState.ConvertToUserOrgStatus(),
            $"Expected error state for invalid initial state of [{invalidInitialState}]"
        );
    }

    [Theory]
    [InlineData(false, UserRole.Super, true)]
    [InlineData(true, UserRole.Champion, true)]
    [InlineData(true, UserRole.Standard, false)]
    [InlineData(false, UserRole.Champion, false)]
    [InlineData(false, UserRole.Standard, false)]
    public async Task ReactivateMembership_AuthorisesBasedOnUserRoleAndOrganisation(
        bool organisationIdMatches,
        UserRole userRole,
        bool expectedAuthorised
    )
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Deactivated
            )
        );
        var harness = new ServiceTestHarness<IOrganisationMembershipService>(
            Context
        ).UpdateCurrentUser(currentUserInfo =>
            currentUserInfo with
            {
                OrganisationId = organisationIdMatches ? userOrgMembership.OrganisationId : 999_999,
                UserRole = userRole,
            }
        );

        var result = await harness.Service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        if (expectedAuthorised)
        {
            result.ShouldBeSuccess();
            return;
        }

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipReactivateUserError.NotAllowed>();
    }

    [Fact]
    public async Task ReactivateMembership_MembershipAlreadyActive_ReturnsOkIdempotently()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );
        var result = await _service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        var dto = result.ShouldBeSuccess();
        dto.Status.ShouldBe(UserOrgStatus.Active);
    }

    [Fact]
    public async Task ReactivateMembership_WhenOrganisationDoesNotExist_ShouldReturnNotFoundResult()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var result = await _service.ReactivateMembership(
            999_999,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipReactivateUserError.NotFound>();
    }

    [Fact]
    public async Task ReactivateMembership_WhenMembershipDoesNotExist_ShouldReturnNotFoundResult()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var result = await _service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            999_999,
            CancellationToken.None
        );

        result.ShouldBeError().ShouldBeOfType<OrganisationMembershipReactivateUserError.NotFound>();
    }

    [Fact]
    public async Task ReactivateMembership_ShouldSendNotificationEmail()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Deactivated
            )
        );
        var result = await _service.ReactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.ShouldBeSuccess();

        var email = _harness
            .Emails.Sent.Single()
            .ShouldBeOfType<ReactivatedUserNotificationEmail>();
        email.OrganisationName.ShouldBe(userOrgMembership.Organisation!.OrganisationName);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnError_WhenTheMembershipBelongsToTheCurrentUser()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        UserRole originalRole = userOrgMembership.UserRole;
        var harness = await CreateHarnessActingAsMembershipOwner(userOrgMembership);

        var result = await harness.Service.UpdateUserRole(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            CancellationToken.None
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.CannotChangeOwnRole>();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(originalRole);
    }

    [Fact]
    public async Task DeactivateMembership_ShouldReturnError_WhenTheMembershipBelongsToTheCurrentUser()
    {
        var userOrgMembership = await SetupUserOrgMembership(
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Active
            )
        );
        var harness = await CreateHarnessActingAsMembershipOwner(userOrgMembership);

        var result = await harness.Service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.CannotDeactivateSelf>();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.Status.ShouldBe(UserOrgMembershipStatus.Active);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task UpdateUserRole_WhenLastEligibleChampion_ShouldRejectWithoutChanges(
        UserOrgStatus status
    )
    {
        // Arrange
        var membershipStatus = (UserOrgMembershipStatus)status;

        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(membershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task DeactivateMembership_WhenLastEligibleChampion_ShouldRejectWithoutChanges(
        UserOrgStatus status
    )
    {
        // Arrange
        var membershipStatus = (UserOrgMembershipStatus)status;

        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        // Act
        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(membershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task UpdateUserRole_WhenAnotherChampionExists_ShouldSucceed(
        UserOrgStatus otherStatus
    )
    {
        // Arrange
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;

        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            otherMembershipStatus
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Standard);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Active);

        // Verify that the other champion membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(UserRole.Champion);
        savedOtherMembership.Status.ShouldBe(otherMembershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task DeactivateMembership_WhenAnotherEligibleChampionExists_ShouldSucceed(
        UserOrgStatus otherStatus
    )
    {
        // Arrange
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;

        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            otherMembershipStatus
        );

        // Act
        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().Status.ShouldBe(UserOrgStatus.Deactivated);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Deactivated);

        // Verify that the other champion membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(UserRole.Champion);
        savedOtherMembership.Status.ShouldBe(otherMembershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Deactivated, UserRole.Champion)]
    [InlineData(UserOrgStatus.Removed, UserRole.Champion)]
    [InlineData(UserOrgStatus.AwaitingSetup, UserRole.Champion)]
    [InlineData(UserOrgStatus.Active, UserRole.Standard)]
    [InlineData(UserOrgStatus.Active, UserRole.Super)]
    public async Task UpdateUserRole_WhenOtherMembershipIsNotEligibleChampion_ShouldRejectWithoutChanges(
        UserOrgStatus otherStatus,
        UserRole otherRole
    )
    {
        // Arrange
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;

        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            otherMembershipStatus,
            otherRole
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);

        _harness.Emails.Sent.ShouldBeEmpty();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Active);

        // Verify that the other membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(otherRole);
        savedOtherMembership.Status.ShouldBe(otherMembershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Deactivated, UserRole.Champion)]
    [InlineData(UserOrgStatus.Removed, UserRole.Champion)]
    [InlineData(UserOrgStatus.AwaitingSetup, UserRole.Champion)]
    [InlineData(UserOrgStatus.Active, UserRole.Standard)]
    [InlineData(UserOrgStatus.Active, UserRole.Super)]
    public async Task DeactivateMembership_WhenOtherMembershipIsNotEligibleChampion_ShouldRejectWithoutChanges(
        UserOrgStatus otherStatus,
        UserRole otherRole
    )
    {
        // Arrange
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;

        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            otherMembershipStatus,
            otherRole
        );

        // Act
        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);

        _harness.Emails.Sent.ShouldBeEmpty();

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Active);

        // Verify that the other membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(otherRole);
        savedOtherMembership.Status.ShouldBe(otherMembershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task UpdateUserRole_WhenChampionRoleIsUnchanged_ShouldSucceed(UserOrgStatus status)
    {
        // Arrange
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Champion },
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Champion);
        membership.Status.ShouldBe(membershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task UpdateUserRole_WhenTargetIsInactiveAndAnotherChampionExists_ShouldSucceed(
        UserOrgStatus otherStatus
    )
    {
        // Arrange
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Inactive
            )
        );

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            (UserOrgMembershipStatus)otherStatus
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Standard);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Inactive);

        // Verify that the other champion membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(UserRole.Champion);
        savedOtherMembership.Status.ShouldBe((UserOrgMembershipStatus)otherStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task DeactivateMembership_WhenTargetIsInactiveAndAnotherChampionExists_ShouldSucceed(
        UserOrgStatus otherStatus
    )
    {
        // Arrange
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Inactive
            )
        );

        var otherMembership = await AddOtherMembership(
            membership.OrganisationId,
            (UserOrgMembershipStatus)otherStatus
        );

        // Act
        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().Status.ShouldBe(UserOrgStatus.Deactivated);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Champion);
        savedMembership.Status.ShouldBe(UserOrgMembershipStatus.Deactivated);

        // Verify that the other champion membership remains unchanged
        var savedOtherMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == otherMembership.Id,
            TestContext.Current.CancellationToken
        );

        savedOtherMembership.UserRole.ShouldBe(UserRole.Champion);
        savedOtherMembership.Status.ShouldBe((UserOrgMembershipStatus)otherStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Deactivated)]
    [InlineData(UserOrgStatus.Removed)]
    [InlineData(UserOrgStatus.AwaitingSetup)]
    public async Task UpdateUserRole_WhenChampionIsNotEligible_ShouldAllowDemotion(
        UserOrgStatus status
    )
    {
        // Arrange
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        var savedMembership = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );

        savedMembership.UserRole.ShouldBe(UserRole.Standard);
        savedMembership.Status.ShouldBe(membershipStatus);
    }

    [Fact]
    public async Task UpdateUserRole_WhenOnlyOtherChampionIsInAnotherOrganisation_ShouldReject()
    {
        // Arrange
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        await AddEntity(
            new UserOrgMembershipFaker()
                .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active)
                .RuleFor(x => x.UserRole, _ => UserRole.Champion)
                .RuleFor(x => x.User, _ => _userFaker.Generate())
                .RuleFor(x => x.Organisation, _ => _organisationFaker.Generate())
                .Generate(),
            TestContext.Current.CancellationToken
        );

        // Act
        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);
    }

    [Fact]
    public async Task DeactivateMembership_WhenOnlyOtherChampionIsInAnotherOrganisation_ShouldReject()
    {
        // Arrange
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        await AddEntity(
            new UserOrgMembershipFaker()
                .RuleFor(x => x.Status, _ => UserOrgMembershipStatus.Active)
                .RuleFor(x => x.UserRole, _ => UserRole.Champion)
                .RuleFor(x => x.User, _ => _userFaker.Generate())
                .RuleFor(x => x.Organisation, _ => _organisationFaker.Generate())
                .Generate(),
            TestContext.Current.CancellationToken
        );

        // Act
        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipDeactivateUserError.LastRemainingChampion>()
            .MembershipId.ShouldBe(membership.Id);
    }

    private async Task<UserOrgMembership> AddOtherMembership(
        int organisationId,
        UserOrgMembershipStatus status,
        UserRole role = UserRole.Champion
    )
    {
        var membership = new UserOrgMembershipFaker()
            .RuleFor(x => x.Status, _ => status)
            .RuleFor(x => x.UserRole, _ => role)
            .RuleFor(x => x.User, _ => _userFaker.Generate())
            .RuleFor(x => x.OrganisationId, _ => organisationId)
            .Generate();
        return await AddEntity(membership, TestContext.Current.CancellationToken);
    }

    // The caller is recognised by the email claim, so acting as the membership's own user means
    // giving the current user the same work email.
    private async Task<
        IServiceTestHarness<IOrganisationMembershipService>
    > CreateHarnessActingAsMembershipOwner(UserOrgMembership membership)
    {
        User owner = await Context.Users.SingleAsync(u => u.Id == membership.UserId);

        return new ServiceTestHarness<IOrganisationMembershipService>(Context).UpdateCurrentUser(
            currentUser => currentUser with { Email = owner.WorkEmail }
        );
    }

    private async Task<UserOrgMembership> SetupUserOrgMembership(
        Action<UserOrgMembership>? modifier = null,
        Faker<UserOrgMembership>? overrideMembershipFaker = null
    )
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        var user = _userFaker.Generate();
        Context.Users.Add(user);
        var organisation = _organisationFaker.Generate();
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

        var userOrgMembership = (overrideMembershipFaker ?? _membershipFaker)
            .Generate()
            .Update(x =>
            {
                x.Id = 123;
                x.UserId = user.Id;
                x.OrganisationId = organisation.Id;
                x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both;
            });
        if (modifier is not null)
        {
            modifier(userOrgMembership);
        }
        Context.UserOrgMemberships.Add(userOrgMembership);
        await Context.SaveChangesAsync();
        return userOrgMembership;
    }
}
