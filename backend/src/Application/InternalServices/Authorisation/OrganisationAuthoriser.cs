using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.InternalServices.Authorisation;

internal class OrganisationAuthoriser : IOrganisationAuthoriser
{
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private static readonly Operation[] _allowedOperationsForStandardUser =
    [
        Operation.Read,
        Operation.Create,
    ];

    public OrganisationAuthoriser(ICurrentUserInfoService currentUserInfoService)
    {
        _currentUserInfoService = currentUserInfoService;
    }

    public ValueOrAll<int> GetAuthorisedOrganisations(Operation operation)
    {
        var currentUser = _currentUserInfoService.GetCurrentUserInfo();

        return currentUser.UserRole switch
        {
            UserRole.Super => ValueOrAll<int>.All,
            _ when IsPermitted(currentUser.UserRole, operation) => currentUser.OrganisationId,
            _ => ValueOrAll<int>.None(),
        };
    }

    public OrganisationAccess GetOrganisationAccess(Operation operation, int organisationId)
    {
        if (GetAuthorisedOrganisations(operation).Contains(organisationId))
        {
            return OrganisationAccess.Allowed;
        }

        var currentUser = _currentUserInfoService.GetCurrentUserInfo();
        bool permittedOnceSelected = currentUser.SelectableMemberships.Any(x =>
            x.OrganisationId == organisationId && IsPermitted(x.UserRole, operation)
        );

        return organisationId != currentUser.OrganisationId && permittedOnceSelected
            ? OrganisationAccess.NotSelected
            : OrganisationAccess.NotAllowed;
    }

    public bool CanPerformOperationOnOrganisation(Operation operation, int organisationId)
    {
        OrganisationAccess access = GetOrganisationAccess(operation, organisationId);
        access.ThrowIfNotSelected(organisationId);
        return access == OrganisationAccess.Allowed;
    }

    private static bool IsPermitted(UserRole role, Operation operation) =>
        role switch
        {
            UserRole.Super or UserRole.Champion => true,
            UserRole.Standard => _allowedOperationsForStandardUser.Contains(operation),
            _ => false,
        };
}
