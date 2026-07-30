namespace UKPS.Api.Application.InternalServices.Identity;

internal class AssociateSoftwareTokenResult
{
    public required string Secret { get; init; }
    public required string AuthenticationSessionId { get; init; }
}
