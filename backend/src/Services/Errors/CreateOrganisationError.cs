namespace UKPS.Api.Services.Errors;

public abstract record CreateOrganisationError
{
    private protected CreateOrganisationError() { }

    // Duplicate organisation name
    public sealed record OrganisationNameConflict() : CreateOrganisationError;

    public sealed record MissingFields() : CreateOrganisationError;
}
