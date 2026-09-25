namespace UKPS.Api.Application.Organisations.Dtos;

/// <summary>
/// Represents the filtering and inclusion options for organisation queries.
/// </summary>
public sealed record OrganisationsQuery
{
    /// <summary>
    /// Gets a value indicating whether organisations that are not authorised should be included in the results.
    /// </summary>
    public bool IncludeNoneAuthorised { get; init; }
}
