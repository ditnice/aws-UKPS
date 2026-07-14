using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Common;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class OrganisationMembershipServiceTests : DatabaseTestBase
{
    private readonly UserFaker _userFaker;
    private readonly UserOrgMembershipFaker _membershipFaker;
    private readonly OrganisationFaker _organisationFaker;
    private readonly IOrganisationMembershipService _service;

    public OrganisationMembershipServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _userFaker = new UserFaker();
        _membershipFaker = new UserOrgMembershipFaker();
        _organisationFaker = new OrganisationFaker();
        _service = new ServiceTestHarness<IOrganisationMembershipService>(Context).Service;
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

        result.IsOk.ShouldBeTrue();
        result.Value.UserRole.ShouldBe(userRole);

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
        var userOrgMembership = await SetupUserOrgMembership();
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
            Assert.True(result.IsOk);
            return;
        }

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipUpdateUserRoleError.NotAllowed>(result.Error);
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

        result.IsErr.ShouldBeTrue();
        result.Error.ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotFound>();
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

        result.IsErr.ShouldBeTrue();
        result.Error.ShouldBeOfType<OrganisationMembershipUpdateUserRoleError.NotFound>();
    }

    [Fact]
    public async Task DeactivateMembership_ShouldDeactivateTheSpecifiedMembership()
    {
        var userOrgMembership = await SetupUserOrgMembership();
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.IsOk.ShouldBeTrue();
        result.Value.Status.ShouldBe(UserOrgStatus.Inactive);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(
            m => m.Id == userOrgMembership.Id,
            TestContext.Current.CancellationToken
        );
        saved.Status.ShouldBe(UserOrgStatus.Inactive);
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
        var userOrgMembership = await SetupUserOrgMembership();
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
            Assert.True(result.IsOk);
            return;
        }

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipDeactivateUserError.NotAllowed>(result.Error);
    }

    [Fact]
    public async Task DeactivateMembership_MembershipAlreadyInactive_ReturnsOkIdempotently()
    {
        var userOrgMembership = await SetupUserOrgMembership(m =>
            m.Status = UserOrgStatus.Inactive
        );
        var result = await _service.DeactivateMembership(
            userOrgMembership.OrganisationId,
            userOrgMembership.Id,
            CancellationToken.None
        );

        result.IsOk.ShouldBeTrue();
        result.Value.Status.ShouldBe(UserOrgStatus.Inactive);
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

        result.IsErr.ShouldBeTrue();
        result.Error.ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotFound>();
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

        result.IsErr.ShouldBeTrue();
        result.Error.ShouldBeOfType<OrganisationMembershipDeactivateUserError.NotFound>();
    }

    private async Task<UserOrgMembership> SetupUserOrgMembership(
        Action<UserOrgMembership>? modifier = null
    )
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        var user = _userFaker.Generate();
        Context.Users.Add(user);
        var organisation = _organisationFaker.Generate();
        Context.Organisations.Add(organisation);
        await Context.SaveChangesAsync();

        var userOrgMembership = _membershipFaker
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
