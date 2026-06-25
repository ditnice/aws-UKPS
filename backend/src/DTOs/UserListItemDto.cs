using UKPS.Api.Enums;

namespace UKPS.Api.DTOs;

public sealed class UserListItemDto
{
    public int UserId { get; init; }
    public string? EmailAddress { get; init; }
    public UserRole Role { get; init; }
    public UserOrgStatus Status { get; init; }
    public DateTime? LastActive { get; init; }
}
