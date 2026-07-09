using UKPS.Api.Enums;
using UKPS.Api.Services.Interfaces;

namespace UKPS.Api.Services;

internal class OrganisationAuthoriser : IOrganisationAuthoriser
{
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private readonly Operation[] _allowedOperationsForStandardUser =
    [
        Operation.View,
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
            UserRole.Champion => currentUser.OrganisationId,
            UserRole.Standard when _allowedOperationsForStandardUser.Contains(operation) =>
                currentUser.OrganisationId,
            _ => ValueOrAll<int>.None(),
        };
    }
}
