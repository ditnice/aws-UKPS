using Bogus;
using Shouldly;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.Users;
using UKPS.Api.Application.Users.Dtos;
using UKPS.Api.Application.Users.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;

namespace UKPS.Api.Tests.Application.Users;

[Collection(DatabaseCollection.Name)]
public class UserServiceTests : DatabaseTestBase
{
    private readonly OrganisationFaker _organisationFaker = new();
    private readonly UserFaker _userFaker = new();
    private readonly UserOrgMembershipFaker _userOrgMembershipFaker = new();
    private readonly UpdateUserDetailsCommandFaker _updateUserDetailsCommandFaker = new();
    private IServiceTestHarness<IUserService> _harness;
    private IUserService Service => _harness.Service;

    private readonly DateTime _currentDateTime = new DateTime(
        2003,
        4,
        12,
        12,
        12,
        44,
        DateTimeKind.Utc
    );

    public UserServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        _harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentTime(
            _currentDateTime
        );
    }

    [Fact]
    public async Task GetUsers_ReturnsOrganisationNotFoundError_WhenOrganisationDoesNotExist()
    {
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(organisationId: 99),
                TestContext.Current.CancellationToken
            );

        result.IsErr.ShouldBeTrue();
        GetUsersError.OrganisationNotFound notFound =
            result.Error.ShouldBeOfType<GetUsersError.OrganisationNotFound>();
        notFound.OrganisationId.ShouldBe(99);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyPage_WhenOrganisationHasNoUsers()
    {
        Context.Organisations.Add(_organisationFaker.Generate());
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(CreateGetUsersQuery(), TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
        dto.Page.ShouldBe(1);
        dto.PageSize.ShouldBe(20);
    }

    [Fact]
    public async Task GetUsers_MapsUserMembershipFields_WhenUsersExist()
    {
        var organisation = _organisationFaker.Generate();
        var lastActive = new DateTime(2026, 6, 19, 12, 50, 1, DateTimeKind.Utc);
        var user = _userFaker.Generate();
        user.Update(x =>
        {
            x.WorkEmail = "user@example.com";
            x.LastActive = lastActive;
        });
        var membership = _userOrgMembershipFaker.Generate();
        membership.Update(x =>
        {
            x.User = user;
            x.Organisation = organisation;
            x.UserRole = UserRole.Champion;
            x.Status = UserOrgStatus.Active;
        });
        Context.Organisations.Add(organisation);
        Context.Users.Add(user);
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(CreateGetUsersQuery(), TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();

        dto.ShouldNotBeNull();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(user.Id);
        item.EmailAddress.ShouldBe(user.WorkEmail);
        item.Role.ShouldBe(UserRole.Champion);
        item.Status.ShouldBe(UserOrgStatus.Active);
        item.LastActive.ShouldBe(lastActive);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleStatuses_WhenStatusesProvided()
    {
        UserOrgStatus[] userOrgStatuses =
        [
            UserOrgStatus.RequestedAccess,
            UserOrgStatus.Active,
            UserOrgStatus.Inactive,
        ];
        Organisation organisation = _organisationFaker.Generate();
        var data = userOrgStatuses.Select(s =>
        {
            var user = _userFaker.Generate();
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            membership.Status = s;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(status: [UserOrgStatus.Active, UserOrgStatus.Inactive]),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([2, 3]);
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_ByDefault()
    {
        UserOrgStatus[] userOrgStatuses =
        [
            UserOrgStatus.Active,
            UserOrgStatus.Rejected,
            UserOrgStatus.Inactive,
        ];
        Organisation organisation = _organisationFaker.Generate();
        var data = userOrgStatuses.Select(s =>
        {
            var user = _userFaker.Generate();
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            membership.Status = s;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(CreateGetUsersQuery(), TestContext.Current.CancellationToken);

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.ShouldAllBe(i => i.Status != UserOrgStatus.Rejected);
    }

    [Fact]
    public async Task GetUsers_ExcludesRejectedUsers_EvenWhenExplicitlyRequested()
    {
        Organisation organisation = _organisationFaker.Generate();
        var user = _userFaker.Generate();
        var membership = _userOrgMembershipFaker.Generate();
        membership.User = user;
        membership.Organisation = organisation;
        membership.Status = UserOrgStatus.Rejected;
        Context.UserOrgMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(status: [UserOrgStatus.Rejected]),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetUsers_FiltersByMultipleRoles_WhenRolesProvided()
    {
        UserRole[] userRoles = [UserRole.Standard, UserRole.Champion, UserRole.Super];
        Organisation organisation = _organisationFaker.Generate();
        var data = userRoles.Select(r =>
        {
            var user = _userFaker.Generate();
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            membership.UserRole = r;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(role: [UserRole.Champion, UserRole.Super]),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([2, 3]);
    }

    [Fact]
    public async Task GetUsers_FiltersByPartialEmail_WhenEmailProvided()
    {
        Organisation organisation = _organisationFaker.Generate();
        string[] emails =
        [
            "john.smith@example.com",
            "jane.doe@example.com",
            "bob.smithers@example.com",
        ];
        var data = emails.Select(e =>
        {
            var user = _userFaker.Generate();
            user.Update(x => x.WorkEmail = e);
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(email: "SMITH"),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.EmailAddress)
            .ShouldBe(["john.smith@example.com", "bob.smithers@example.com"]);
    }

    [Fact]
    public async Task GetUsers_TreatsLikeWildcardsLiterally_WhenEmailContainsPercentOrUnderscore()
    {
        Organisation organisation = _organisationFaker.Generate();
        string[] emails = ["100%off@example.com", "jane_doe@example.com", "john.smith@example.com"];
        var data = emails.Select(e =>
        {
            var user = _userFaker.Generate();
            user.Update(x => x.WorkEmail = e);
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(email: "100%off"),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("100%off@example.com");
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveRange_WhenBothBoundsProvided()
    {
        Organisation organisation = _organisationFaker.Generate();
        (DateTime? LastActive, string Email)[] users =
        [
            (new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "before@example.com"),
            (new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), "inrange@example.com"),
            (new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), "after@example.com"),
        ];
        var data = users.Select(u =>
        {
            var user = _userFaker.Generate();
            user.Update(x =>
            {
                x.WorkEmail = u.Email;
                x.LastActive = u.LastActive;
            });
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(
                    lastActiveFrom: new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
                    lastActiveTo: new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)
                ),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("inrange@example.com");
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveFrom_WhenOnlyFromProvided()
    {
        Organisation organisation = _organisationFaker.Generate();
        (DateTime? LastActive, string Email)[] users =
        [
            (new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "before@example.com"),
            (new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), "after@example.com"),
        ];
        var data = users.Select(u =>
        {
            var user = _userFaker.Generate();
            user.Update(x =>
            {
                x.WorkEmail = u.Email;
                x.LastActive = u.LastActive;
            });
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(
                    lastActiveFrom: new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)
                ),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("after@example.com");
    }

    [Fact]
    public async Task GetUsers_FiltersByLastActiveTo_WhenOnlyToProvided()
    {
        Organisation organisation = _organisationFaker.Generate();
        (DateTime? LastActive, string Email)[] users =
        [
            (new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "before@example.com"),
            (new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), "after@example.com"),
        ];
        var data = users.Select(u =>
        {
            var user = _userFaker.Generate();
            user.Update(x =>
            {
                x.WorkEmail = u.Email;
                x.LastActive = u.LastActive;
            });
            var membership = _userOrgMembershipFaker.Generate();
            membership.User = user;
            membership.Organisation = organisation;
            return membership;
        });
        Context.UserOrgMemberships.AddRange(data);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(
                    lastActiveTo: new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)
                ),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("before@example.com");
    }

    [Fact]
    public async Task GetUsers_ExcludesNeverActiveUsers_WhenLastActiveFilterProvided()
    {
        Organisation organisation = _organisationFaker.Generate();
        var neverActiveUser = _userFaker.Generate();
        neverActiveUser.Update(x =>
        {
            x.WorkEmail = "neveractive@example.com";
            x.LastActive = null;
        });
        var activeUser = _userFaker.Generate();
        activeUser.Update(x =>
        {
            x.WorkEmail = "active@example.com";
            x.LastActive = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        });
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = neverActiveUser;
                    x.Organisation = organisation;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = activeUser;
                    x.Organisation = organisation;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(
                    lastActiveFrom: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
                ),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.EmailAddress.ShouldBe("active@example.com");
    }

    [Fact]
    public async Task GetUsers_PaginatesAndOrdersByUserId_WhenUsersExist()
    {
        Context.Organisations.Add(_organisationFaker.Generate().Update(x => x.Id = 1));
        Context.Users.AddRange(
            _userFaker.Generate().Update(x => x.Id = 30),
            _userFaker.Generate().Update(x => x.Id = 10),
            _userFaker.Generate().Update(x => x.Id = 20)
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 30;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 3;
                    x.UserId = 20;
                    x.OrganisationId = 1;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(page: 2, pageSize: 1),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(3);
        dto.Page.ShouldBe(2);
        dto.PageSize.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersAcrossOrganisations_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.AddRange(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "one@example.com";
                }),
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 20;
                    x.WorkEmail = "two@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 20;
                    x.OrganisationId = 2;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(organisationId: null),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 20]);
    }

    [Fact]
    public async Task GetUsers_FiltersByStatus_WhenOrganisationIdIsMissing()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.AddRange(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "active@example.com";
                }),
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 20;
                    x.WorkEmail = "inactive@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                    x.Status = UserOrgStatus.Active;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 20;
                    x.OrganisationId = 2;
                    x.Status = UserOrgStatus.Inactive;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(organisationId: null, status: [UserOrgStatus.Inactive]),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(1);
        UserListItemDto item = dto.Items.ShouldHaveSingleItem();
        item.UserId.ShouldBe(20);
        item.Status.ShouldBe(UserOrgStatus.Inactive);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        Context.Organisations.Add(_organisationFaker.Generate().Update(x => x.Id = 1));
        Context.Users.Add(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "user@example.com";
                })
        );
        Context.UserOrgMemberships.Add(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                    x.Status = UserOrgStatus.Active;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(page: 5),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto>? dto = result.ShouldBeSuccess();
        dto.Items.ShouldBeEmpty();
        dto.TotalCount.ShouldBe(1);
        dto.Page.ShouldBe(5);
    }

    [Fact]
    public async Task GetUsers_UserHasMembershipsInMultipleOrganisations_ReturnsOneRowPerMembership()
    {
        Context.Organisations.AddRange(
            _organisationFaker.Generate().Update(x => x.Id = 1),
            _organisationFaker.Generate().Update(x => x.Id = 2)
        );
        Context.Users.Add(
            _userFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 10;
                    x.WorkEmail = "multi@example.com";
                })
        );
        Context.UserOrgMemberships.AddRange(
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 1;
                    x.UserId = 10;
                    x.OrganisationId = 1;
                    x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines;
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.Id = 2;
                    x.UserId = 10;
                    x.OrganisationId = 2;
                    x.AllowedPharmaceuticalEntity = PharmaceuticalEntity.Medicines;
                })
        );
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await Service.GetUsers(
                CreateGetUsersQuery(organisationId: null),
                TestContext.Current.CancellationToken
            );

        PaginatedResponseDto<UserListItemDto> dto = result.ShouldBeSuccess();
        dto.TotalCount.ShouldBe(2);
        dto.Items.Select(i => i.UserId).ToArray().ShouldBe([10, 10]);
    }

    [Theory]
    [InlineData(UserRole.Super, false)]
    [InlineData(UserRole.Champion, true)]
    [InlineData(UserRole.Standard, true)]
    public async Task GetUsers_ReturnsAllUsersForSuperAdmins_AndFiltersByOrganisationForOtherRoles(
        UserRole userRole,
        bool filtersByOrganisation
    )
    {
        var organisations = _organisationFaker.Generate(2);
        var users = _userFaker.Generate(3);
        var memberships = new List<UserOrgMembership>
        {
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[0];
                    x.Organisation = organisations[0];
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[1];
                    x.Organisation = organisations[1];
                }),
            _userOrgMembershipFaker
                .Generate()
                .Update(x =>
                {
                    x.User = users[2];
                    x.Organisation = organisations[0];
                }),
        };
        await AddEntities(memberships, TestContext.Current.CancellationToken);
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = organisations[0].Id,
                UserRole = userRole,
            }
        );
        var results = await harness.Service.GetUsers(
            CreateGetUsersQuery(organisationId: null),
            TestContext.Current.CancellationToken
        );

        var dto = results.ShouldBeSuccess();

        if (filtersByOrganisation)
        {
            dto.TotalCount.ShouldBe(2);
            dto.Items.Select(i => i.UserId).ToArray().ShouldBe([users[0].Id, users[2].Id]);
        }
        else
        {
            dto.TotalCount.ShouldBe(3);
            dto.Items.Select(i => i.UserId)
                .ToArray()
                .ShouldBe([users[0].Id, users[1].Id, users[2].Id]);
        }
    }

    [Theory]
    [InlineData(UserRole.Super, true)]
    [InlineData(UserRole.Champion, false)]
    [InlineData(UserRole.Standard, false)]
    public async Task GetUsers_ReturnsNotAllowed_WhenExplicitlyRequestingUsersForAnOrganisationIsNotAllowedToAccess(
        UserRole userRole,
        bool isAllowedToAccess
    )
    {
        int userOrganisation = 1;
        int otherOrganisation = 2;
        var harness = new ServiceTestHarness<IUserService>(Context).UpdateCurrentUser(x =>
            x with
            {
                UserRole = userRole,
                OrganisationId = userOrganisation,
            }
        );
        IUserService service = harness.Service;
        Result<PaginatedResponseDto<UserListItemDto>, GetUsersError> result =
            await service.GetUsers(
                CreateGetUsersQuery(organisationId: otherOrganisation),
                TestContext.Current.CancellationToken
            );

        if (isAllowedToAccess)
        {
            result.Error.ShouldNotBeOfType<GetUsersError.NotAllowed>();
        }
        else
        {
            result.Error.ShouldBeOfType<GetUsersError.NotAllowed>();
        }
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldUpdateUserDetails()
    {
        var currentUser = await CreateExistingCurrentUser();
        var command = _updateUserDetailsCommandFaker.Generate();
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        var databaseUser = await Context.Users.FindAsync(
            [currentUser.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        var databaseValues = new UpdateUserDetailsCommand()
        {
            FullName = databaseUser.FullName,
            WorkEmail = databaseUser.WorkEmail,
            WorkTelephone = databaseUser.WorkTelephone,
        };
        databaseValues.ShouldBe(command);
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldUpdateUpdatedAtTime()
    {
        var currentUser = await CreateExistingCurrentUser();
        var command = _updateUserDetailsCommandFaker.Generate();
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        var databaseUser = await Context.Users.FindAsync(
            [currentUser.Id],
            TestContext.Current.CancellationToken
        );
        databaseUser.ShouldNotBeNull();
        databaseUser.UpdatedAt.ShouldBe(_currentDateTime);
    }

    [Fact]
    public async Task UpdateUserDetails_ShouldReturnUpdatedUserDetails()
    {
        User currentUser = await CreateExistingCurrentUser();
        UpdateUserDetailsCommand command = _updateUserDetailsCommandFaker.Generate();
        Result<UserDetailsDto, UpdateUserDetailsError> result = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );

        UserDetailsDto value = result.ShouldBeSuccess();
        var responseValues = new UpdateUserDetailsCommand()
        {
            FullName = value.FullName,
            WorkEmail = value.WorkEmail,
            WorkTelephone = value.WorkPhone,
        };
        responseValues.ShouldBe(command);
    }

    [Fact]
    public async Task UpdateUserDetails_WhenEmailChanges_ShouldUpdateEmailAttributeInCognito()
    {
        User currentUser = await CreateExistingCurrentUser();
        string testEmail = "testupdateuserdetails@email.com";
        UpdateUserDetailsCommand command = _updateUserDetailsCommandFaker.Generate() with
        {
            WorkEmail = testEmail,
        };
        _ = await Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );
        MockUser? user = _harness.Cognito.GetUserByEmail(testEmail);
        user.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateUserDetails_WhenUserDoesNotExist_ShouldReturnUserDoesNotExistError()
    {
        var result = await Service.UpdateUserDetails(
            999,
            _updateUserDetailsCommandFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UpdateUserDetailsError.UserDoesNotExist>();
    }

    [Fact]
    public async Task UpdateUserDetails_WhenUserIsNotTheTargetUser_ShouldReturnUserIsNoPermittedToUserDetailsError()
    {
        var currentUser = await CreateExistingCurrentUser();
        var otherUserTestHarness = new ServiceTestHarness<IUserService>(_harness).UpdateCurrentUser(
            x => x with { Email = "otheruser@email.com" }
        );
        var command = _updateUserDetailsCommandFaker.Generate();
        var result = await otherUserTestHarness.Service.UpdateUserDetails(
            currentUser.Id,
            command,
            TestContext.Current.CancellationToken
        );
        result.ShouldBeError().ShouldBeOfType<UpdateUserDetailsError.Unauthorised>();
    }

    private async Task<User> CreateExistingCurrentUser()
    {
        Organisation org = await AddEntity(
            _organisationFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        IUserAdministrationService userAdministrationService =
            new ServiceTestHarness<IUserAdministrationService>(_harness).Service;
        Faker<OnboardUserCommandDto> onboardingUserFaker = new OnboardUserCommandDtoFaker().RuleFor(
            x => x.OrganisationId,
            _ => org.Id
        );
        Result<int, OnboardUserError> result = await userAdministrationService.OnboardUser(
            onboardingUserFaker.Generate(),
            TestContext.Current.CancellationToken
        );
        return result.Match(
            x =>
            {
                var user =
                    Context.Users.Find(x)
                    ?? throw new InvalidOperationException("Could not find created user.");
                _harness = _harness.UpdateCurrentUser(x => x with { Email = user.WorkEmail });

                return user;
            },
            (e) => throw new InvalidOperationException("Failed to create an initial user")
        );
    }

    private static GetUsersQueryDto CreateGetUsersQuery(
        int? organisationId = 1,
        int page = 1,
        int pageSize = 20,
        ICollection<UserOrgStatus>? status = null,
        ICollection<UserRole>? role = null,
        string? email = null,
        DateTimeOffset? lastActiveFrom = null,
        DateTimeOffset? lastActiveTo = null
    ) =>
        new()
        {
            OrganisationId = organisationId,
            Page = page,
            PageSize = pageSize,
            Status = status ?? [],
            Role = role ?? [],
            Email = email,
            LastActiveFrom = lastActiveFrom,
            LastActiveTo = lastActiveTo,
        };
}
