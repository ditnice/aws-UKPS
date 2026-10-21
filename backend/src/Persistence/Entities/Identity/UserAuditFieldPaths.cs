namespace UKPS.Api.Persistence.Entities.Identity;

/// <summary>
/// Canonical property-name paths for user audit fields. Future field-level audit writers should
/// use property names in this format.
/// </summary>
internal static class UserAuditFieldPaths
{
    public const string Title = nameof(User.Title);
    public const string FullName = nameof(User.FullName);
    public const string JobTitle = nameof(User.JobTitle);
    public const string WorkTelephone = nameof(User.WorkTelephone);
    public const string WorkEmail = nameof(User.WorkEmail);

    public static readonly string[] PersonalFields =
    [
        Title,
        FullName,
        JobTitle,
        WorkTelephone,
        WorkEmail,
    ];
}
