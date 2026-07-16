public abstract record CreateUserError
{
    private protected CreateUserError() { }

    internal sealed record NotFound(int OrganisationId) : CreateUserError;
}
