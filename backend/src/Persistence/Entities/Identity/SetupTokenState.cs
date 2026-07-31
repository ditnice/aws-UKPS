namespace UKPS.Api.Persistence.Entities.Identity;

internal enum SetupTokenState
{
    Valid = 0,
    Consumed = 1,
    Expired = 2,
}
