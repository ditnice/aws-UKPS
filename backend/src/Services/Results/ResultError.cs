namespace UKPS.Api.Services.Results;

public sealed record ResultError(string Id, ErrorType Type, string Description);
