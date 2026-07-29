namespace UKPS.Api.Application.InternalServices.Identity;

internal abstract record CreateNewUserError
{
    protected CreateNewUserError() { }

    public sealed record UsernameAlreadyExists : CreateNewUserError;
}
