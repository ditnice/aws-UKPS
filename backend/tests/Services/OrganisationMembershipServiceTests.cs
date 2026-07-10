using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Tests.Fixtures;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Data.Fakers;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class OrganisationMembershipServiceTests : DatabaseTestBase
{
    private readonly OrganisationMembershipService _service;
    private readonly UserFaker _userFaker;
    private readonly UserOrgMembershipFaker _membershipFaker;
    private readonly OrganisationFaker _organisationFaker;

    public OrganisationMembershipServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _service = new OrganisationMembershipService(Context);
        _userFaker = new UserFaker();
        _membershipFaker = new UserOrgMembershipFaker();
        _organisationFaker = new OrganisationFaker();
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
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == userOrgMembership.Id
        );
        saved.UserRole.ShouldBe(userRole);
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
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == userOrgMembership.Id
        );
        saved.Status.ShouldBe(UserOrgStatus.Inactive);
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
