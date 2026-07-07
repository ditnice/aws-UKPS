using Microsoft.EntityFrameworkCore;
using UKPS.Api.DTOs;
using UKPS.Api.Entities.Identity;
using UKPS.Api.Enums;
using UKPS.Api.Services;

namespace UKPS.Api.Tests.Services;

public class OrganisationMembershipServiceTests : IAsyncDisposable
{
    private readonly TestDatabase _testDatabase;
    private readonly OrganisationMembershipService _service;

    public OrganisationMembershipServiceTests()
    {
        _testDatabase = new TestDatabase();
        _service = new OrganisationMembershipService(_testDatabase.Context);
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

        Assert.NotNull(result);
        Assert.Equal(userRole, result.UserRole);
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

        Assert.Null(result);
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

        Assert.Null(result);
    }

    private async Task<UserOrgMembership> SetupUserOrgMembership(
        Action<UserOrgMembership>? modifier = null
    )
    {
        var userOrgMembership = new UserOrgMembership()
        {
            Id = 123,
            UserId = 234,
            OrganisationId = 345,
            AllowedPharmaceuticalEntity = PharmaceuticalEntity.Both,
        };
        if (modifier is not null)
        {
            modifier(userOrgMembership);
        }
        _testDatabase.Context.UserOrgMemberships.Add(userOrgMembership);
        await _testDatabase.Context.SaveChangesAsync();
        return userOrgMembership;
    }

    public async ValueTask DisposeAsync()
    {
        await _testDatabase.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
