using UKPS.Api.Persistence.Enums;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.Persistence.Data.Seeding;

/// <summary>
/// Fixed users in the dev Champion's organisation for the E2E tests that run as a Champion to act
/// on, so they never pick the same user as a test running as another role. The frontend E2E tests
/// rely on these emails (frontend/tests/e2e/helpers/subjects.ts).
/// </summary>
internal static class E2eSubjectUsers
{
    internal sealed record Subject(string Email, UserRole Role, UserOrgMembershipStatus Status);

    public static int OrganisationId => DevAuthenticationClaims.RoleOrganisationId;

    public static IReadOnlyList<Subject> All { get; } =
    [
        // Promoted to champion and demoted back.
        new("e2e.subject.promote@email.com", UserRole.Standard, UserOrgMembershipStatus.Active),
        // Deactivated and reactivated.
        new("e2e.subject.deactivate@email.com", UserRole.Standard, UserOrgMembershipStatus.Active),
        // Only opened by tests that cancel, so it never changes.
        new("e2e.subject.readonly@email.com", UserRole.Standard, UserOrgMembershipStatus.Active),
        new("e2e.subject.champion@email.com", UserRole.Champion, UserOrgMembershipStatus.Active),
        new(
            "e2e.subject.deactivated@email.com",
            UserRole.Standard,
            UserOrgMembershipStatus.Deactivated
        ),
    ];
}
