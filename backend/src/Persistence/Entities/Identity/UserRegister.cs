namespace UKPS.Api.Persistence.Entities.Identity;

internal sealed class UserRegister
{
    public int Id { get; set; }
    public required string Organisation { get; set; }
    public required string FullName { get; set; }
    public required string WorkEmail { get; set; }
    public required string PhoneNumber { get; set; }
}
