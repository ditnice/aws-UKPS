namespace UKPS.Api.Tests.Utilities.Harnesses;

internal sealed record MockUser
{
    public required string Username { get; init; }
    public required string SubjectId { get; init; }
    public required string Email { get; init; }
    public string? Password { get; init; }
    public bool MfaSetup { get; init; }
}
