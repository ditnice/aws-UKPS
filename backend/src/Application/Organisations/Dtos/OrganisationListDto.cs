namespace UKPS.Api.Application.Organisations.Dtos;

/// <summary>
/// Represents the ID and name of an organisation.
/// </summary>
public sealed record OrganisationListDto
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string OrganisationName { get; init; }
}
