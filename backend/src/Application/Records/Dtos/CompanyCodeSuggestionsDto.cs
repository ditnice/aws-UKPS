namespace UKPS.Api.Application.Records.Dtos;

/// <summary>
/// The company codes already used on an organisation's records, offered as suggestions while a
/// user types a company code.
/// </summary>
public sealed record CompanyCodeSuggestionsDto
{
    /// <summary>
    /// Gets the distinct matching company codes, those starting with the search first, then
    /// alphabetically.
    /// </summary>
    public required IReadOnlyList<string> Items { get; init; }
}
