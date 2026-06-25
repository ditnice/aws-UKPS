using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public sealed class GetUsersQueryDto
{
    [Required]
    public int? OrganisationId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page cannot be less than 1.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 20;

    public ICollection<UserOrgStatus> Status { get; init; } = new Collection<UserOrgStatus>();
}
