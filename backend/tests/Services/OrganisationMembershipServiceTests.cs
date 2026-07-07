using Microsoft.EntityFrameworkCore;
using UKPS.Api.Data;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;
using UKPS.Api.Services.Errors;
using UKPS.Api.Tests.Fixtures;

namespace UKPS.Api.Tests.Services;

[Collection(DatabaseCollection.Name)]
public class OrganisationMembershipServiceTests : DatabaseTestBase
{
    private readonly OrganisationMembershipService _service;

    public OrganisationMembershipServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _service = new OrganisationMembershipService(Context);
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

        Assert.True(result.IsOk);
        Assert.Equal(userRole, result.Value.UserRole);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == userOrgMembership.Id
        );
        Assert.Equal(userRole, saved.UserRole);
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

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipUpdateUserRoleError.NotFound>(result.Error);
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

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipUpdateUserRoleError.NotFound>(result.Error);
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

        Assert.True(result.IsOk);
        Assert.Equal(UserOrgStatus.Inactive, result.Value.Status);

        await using AppDbContext verifyContext = Fixture.CreateContext();
        UserOrgMembership saved = await verifyContext.UserOrgMemberships.SingleAsync(m =>
            m.Id == userOrgMembership.Id
        );
        Assert.Equal(UserOrgStatus.Inactive, saved.Status);
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

        Assert.True(result.IsOk);
        Assert.Equal(UserOrgStatus.Inactive, result.Value.Status);
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

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipDeactivateUserError.NotFound>(result.Error);
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

        Assert.True(result.IsErr);
        Assert.IsType<OrganisationMembershipDeactivateUserError.NotFound>(result.Error);
    }

    private async Task<UserOrgMembership> SetupUserOrgMembership(
        Action<UserOrgMembership>? modifier = null
    )
    {
        // Both FKs are Restrict, so the parent User and Organisation rows must exist first.
        Context.Users.Add(EntityFactory.CreateUser(id: 234, workEmail: "member@example.com"));
        Context.Organisations.Add(EntityFactory.CreateOrganisation(id: 345));
        await Context.SaveChangesAsync();

        var userOrgMembership = EntityFactory.CreateMembership(
            id: 123,
            userId: 234,
            organisationId: 345,
            allowedPharmaceuticalEntity: PharmaceuticalEntity.Both
        );
        if (modifier is not null)
        {
            modifier(userOrgMembership);
        }
        Context.UserOrgMemberships.Add(userOrgMembership);
        await Context.SaveChangesAsync();
        return userOrgMembership;
    }
}
