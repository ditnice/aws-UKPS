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
        var userOrgMembership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
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
            modifier: x => x.UserRole = UserRole.Champion,
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
    [InlineData(false, UserOrgStatus.Active)]
    [InlineData(false, UserOrgStatus.Inactive)]
    [InlineData(true, UserOrgStatus.Active)]
    [InlineData(true, UserOrgStatus.Inactive)]
    public async Task ChampionLoss_WhenLastEligibleChampion_ShouldRejectWithoutChanges(
        bool deactivate,
        UserOrgStatus status
    )
    {
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        await AssertChampionLossBlocked(membership, deactivate);

        membership.UserRole.ShouldBe(UserRole.Champion);
        membership.Status.ShouldBe(membershipStatus);
        await using AppDbContext verifyContext = Fixture.CreateContext();
        var saved = await verifyContext.UserOrgMemberships.SingleAsync(
            x => x.Id == membership.Id,
            TestContext.Current.CancellationToken
        );
        saved.UserRole.ShouldBe(UserRole.Champion);
        saved.Status.ShouldBe(membershipStatus);
        _harness.Emails.Sent.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, UserOrgStatus.Active, UserRole.Champion, true)]
    [InlineData(false, UserOrgStatus.Inactive, UserRole.Champion, true)]
    [InlineData(false, UserOrgStatus.Deactivated, UserRole.Champion, false)]
    [InlineData(false, UserOrgStatus.Removed, UserRole.Champion, false)]
    [InlineData(false, UserOrgStatus.AwaitingSetup, UserRole.Champion, false)]
    [InlineData(false, UserOrgStatus.Active, UserRole.Standard, false)]
    [InlineData(false, UserOrgStatus.Active, UserRole.Super, false)]
    [InlineData(true, UserOrgStatus.Active, UserRole.Champion, true)]
    [InlineData(true, UserOrgStatus.Inactive, UserRole.Champion, true)]
    [InlineData(true, UserOrgStatus.Deactivated, UserRole.Champion, false)]
    [InlineData(true, UserOrgStatus.Removed, UserRole.Champion, false)]
    [InlineData(true, UserOrgStatus.AwaitingSetup, UserRole.Champion, false)]
    [InlineData(true, UserOrgStatus.Active, UserRole.Standard, false)]
    [InlineData(true, UserOrgStatus.Active, UserRole.Super, false)]
    public async Task ChampionLoss_ShouldRequireAnotherEligibleChampion(
        bool deactivate,
        UserOrgStatus otherStatus,
        UserRole otherRole,
        bool allowed
    )
    {
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var other = await AddOtherMembership(
            membership.OrganisationId,
            otherMembershipStatus,
            otherRole
        );

        if (allowed)
        {
            await AssertChampionLossAllowed(membership, deactivate);
        }
        else
        {
            await AssertChampionLossBlocked(membership, deactivate);
        }

        other.Status.ShouldBe(otherMembershipStatus);
        other.UserRole.ShouldBe(otherRole);
    }

    [Theory]
    [InlineData(UserOrgStatus.Active)]
    [InlineData(UserOrgStatus.Inactive)]
    public async Task UpdateUserRole_WhenChampionRoleIsUnchanged_ShouldSucceed(UserOrgStatus status)
    {
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        var result = await _service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Champion },
            TestContext.Current.CancellationToken
        );

        result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Champion);
        membership.Status.ShouldBe(membershipStatus);
    }

    [Theory]
    [InlineData(false, UserOrgStatus.Active)]
    [InlineData(false, UserOrgStatus.Inactive)]
    [InlineData(true, UserOrgStatus.Active)]
    [InlineData(true, UserOrgStatus.Inactive)]
    public async Task ChampionLoss_WhenTargetIsInactiveAndAnotherChampionExists_ShouldSucceed(
        bool deactivate,
        UserOrgStatus otherStatus
    )
    {
        var otherMembershipStatus = (UserOrgMembershipStatus)otherStatus;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(
                x => x.Status,
                _ => UserOrgMembershipStatus.Inactive
            )
        );
        await AddOtherMembership(membership.OrganisationId, otherMembershipStatus);

        await AssertChampionLossAllowed(membership, deactivate);
    }

    [Theory]
    [InlineData(UserOrgStatus.Deactivated)]
    [InlineData(UserOrgStatus.Removed)]
    [InlineData(UserOrgStatus.AwaitingSetup)]
    public async Task UpdateUserRole_WhenChampionIsNotEligible_ShouldAllowDemotion(
        UserOrgStatus status
    )
    {
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        await AssertChampionLossAllowed(membership, deactivate: false);

        membership.Status.ShouldBe(membershipStatus);
    }

    [Theory]
    [InlineData(UserOrgStatus.Deactivated, true)]
    [InlineData(UserOrgStatus.Removed, false)]
    [InlineData(UserOrgStatus.AwaitingSetup, false)]
    public async Task DeactivateMembership_WhenChampionIsNotEligible_ShouldPreserveStateHandling(
        UserOrgStatus status,
        bool allowed
    )
    {
        var membershipStatus = (UserOrgMembershipStatus)status;
        var membership = await SetupUserOrgMembership(
            modifier: x => x.UserRole = UserRole.Champion,
            overrideMembershipFaker: _membershipFaker.RuleFor(x => x.Status, _ => membershipStatus)
        );

        var result = await _service.DeactivateMembership(
            membership.OrganisationId,
            membership.Id,
            TestContext.Current.CancellationToken
        );

        if (allowed)
        {
            result.ShouldBeSuccess();
        }
        else
        {
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotAllowedInCurrentState>();
        }
        membership.Status.ShouldBe(membershipStatus);
        _harness.Emails.Sent.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChampionLoss_WhenActionsAreSequential_ShouldBlockTheSecondAction(
        bool deactivate
    )
    {
        var first = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var last = await AddOtherMembership(first.OrganisationId, UserOrgMembershipStatus.Inactive);

        await AssertChampionLossAllowed(first, deactivate);
        await AssertChampionLossBlocked(last, deactivate);

        last.UserRole.ShouldBe(UserRole.Champion);
        last.Status.ShouldBe(UserOrgMembershipStatus.Inactive);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChampionLoss_WhenChampionBelongsToAnotherOrganisation_ShouldStillReject(
        bool deactivate
    )
    {
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var otherOrganisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        await AddOtherMembership(otherOrganisation.Id, UserOrgMembershipStatus.Active);

        await AssertChampionLossBlocked(membership, deactivate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChampionLoss_WhenMembershipIsOutsideRequestedOrganisation_ShouldReturnNotFound(
        bool deactivate
    )
    {
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var otherOrganisation = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );

        if (deactivate)
        {
            var result = await _service.DeactivateMembership(
                otherOrganisation.Id,
                membership.Id,
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotFound>();
        }
        else
        {
            var result = await _service.UpdateUserRole(
                otherOrganisation.Id,
                membership.Id,
                new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotFound>();
        }
        membership.UserRole.ShouldBe(UserRole.Champion);
        membership.Status.ShouldBe(UserOrgMembershipStatus.Active);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChampionLoss_WhenCallerIsNotAuthorised_ShouldPreservePermissionPrecedence(
        bool deactivate
    )
    {
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var service = new ServiceTestHarness<IOrganisationMembershipService>(Context)
            .UpdateCurrentUser(x => x with { UserRole = UserRole.Standard })
            .Service;

        if (deactivate)
        {
            var result = await service.DeactivateMembership(
                membership.OrganisationId,
                membership.Id,
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotAllowed>();
        }
        else
        {
            var result = await service.UpdateUserRole(
                membership.OrganisationId,
                membership.Id,
                new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotAllowed>();
        }
    }

    [Fact]
    public async Task UpdateUserRole_WhenLastChampionIsPromotedByNonSuper_ShouldPreserveSuperRoleRestriction()
    {
        var membership = await SetupUserOrgMembership(x => x.UserRole = UserRole.Champion);
        var service = new ServiceTestHarness<IOrganisationMembershipService>(Context)
            .UpdateCurrentUser(x =>
                x with
                {
                    OrganisationId = membership.OrganisationId,
                    UserRole = UserRole.Champion,
                }
            )
            .Service;

        var result = await service.UpdateUserRole(
            membership.OrganisationId,
            membership.Id,
            new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Super },
            TestContext.Current.CancellationToken
        );

        result
            .ShouldBeError()
            .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.CannotManageSuperRole>();
    }

    private async Task AssertChampionLossBlocked(UserOrgMembership membership, bool deactivate)
    {
        if (deactivate)
        {
            var result = await _service.DeactivateMembership(
                membership.OrganisationId,
                membership.Id,
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipDeactivateUserError.LastRemainingChampion>()
                .MembershipId.ShouldBe(membership.Id);
        }
        else
        {
            var result = await _service.UpdateUserRole(
                membership.OrganisationId,
                membership.Id,
                new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
                TestContext.Current.CancellationToken
            );
            result
                .ShouldBeError()
                .ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.LastRemainingChampion>()
                .MembershipId.ShouldBe(membership.Id);
        }
    }

    private async Task AssertChampionLossAllowed(UserOrgMembership membership, bool deactivate)
    {
        if (deactivate)
        {
            var result = await _service.DeactivateMembership(
                membership.OrganisationId,
                membership.Id,
                TestContext.Current.CancellationToken
            );
            result.ShouldBeSuccess().Status.ShouldBe(UserOrgStatus.Deactivated);
        }
        else
        {
            var result = await _service.UpdateUserRole(
                membership.OrganisationId,
                membership.Id,
                new UpdateOrgMembershipUserRoleCommandDto() { UserRole = UserRole.Standard },
                TestContext.Current.CancellationToken
            );
            result.ShouldBeSuccess().UserRole.ShouldBe(UserRole.Standard);
        }
    }

    private async Task<UserOrgMembership> AddOtherMembership(
        int organisationId,
        UserOrgMembershipStatus status,
        UserRole role = UserRole.Champion
    )
    {
        var user = await AddEntity(_userFaker.Generate(), TestContext.Current.CancellationToken);
        var membership = new UserOrgMembershipFaker()
            .RuleFor(x => x.Status, _ => status)
            .RuleFor(x => x.UserRole, _ => role)
            .Generate()
            .Update(x =>
            {
                x.UserId = user.Id;
                x.OrganisationId = organisationId;
            });
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
