using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.InternalServices.Authorisation;

internal class OrganisationAuthoriser : IOrganisationAuthoriser
{
    private readonly ICurrentUserInfoService _currentUserInfoService;
    private static readonly Operation[] _allowedOperationsForStandardUser =
    [
        Operation.Read,
        Operation.CreateRecord,
        Operation.EditRecordContent,
        Operation.ChangeRecordStatus,
        Operation.DiscardRecord,
        Operation.ConfirmRecord,
    ];

    // Super users can act on every organisation but can't create, discard or confirm records, or
    // change their content or status.
    private static readonly Operation[] _deniedOperationsForSuperUser =
    [
        Operation.CreateRecord,
        Operation.EditRecordContent,
        Operation.ChangeRecordStatus,
        Operation.DiscardRecord,
        Operation.ConfirmRecord,
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
