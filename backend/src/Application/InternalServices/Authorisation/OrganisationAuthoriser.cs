using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Enums;

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
            UserRole.Champion => currentUser.OrganisationId,
            UserRole.Standard when _allowedOperationsForStandardUser.Contains(operation) =>
                currentUser.OrganisationId,
            _ => ValueOrAll<int>.None(),
        };
    }
}
