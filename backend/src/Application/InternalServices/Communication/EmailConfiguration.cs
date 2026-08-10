using System.ComponentModel.DataAnnotations;

namespace UKPS.Api.Application.InternalServices.Communication;

internal record EmailConfiguration
{
    public const string SectionName = "Email";

    public string FromAddress => $"{FromAddressPrefix}@{BaseDomain}";

    [Required]
    public required string BaseDomain { get; init; }

    [Required]
    public required string FromAddressPrefix { get; init; }
}
