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
        Operation.EditRecordContent,
        Operation.ChangeRecordStatus,
    ];

    // Super users can act on every organisation but can't change record content or status.
    private static readonly Operation[] _deniedOperationsForSuperUser =
    [
        Operation.EditRecordContent,
        Operation.ChangeRecordStatus,
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
            UserRole.Super when _deniedOperationsForSuperUser.Contains(operation) =>
                ValueOrAll<int>.None(),
            UserRole.Super => ValueOrAll<int>.All,
            UserRole.Champion => currentUser.OrganisationId,
            UserRole.Standard when _allowedOperationsForStandardUser.Contains(operation) =>
                currentUser.OrganisationId,
            _ => ValueOrAll<int>.None(),
        };
    }
}
