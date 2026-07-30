using Microsoft.Extensions.Options;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;

namespace UKPS.Api.Application.AuthorisationAdministration;

internal class AuthorisationAdministrationService : IAuthorisationAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly IOptions<UserOnboardingConfiguration> _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthorisationAdministrationService(
        AppDbContext appDbContext,
        IOptions<UserOnboardingConfiguration> options,
        IDateTimeProvider dateTimeProvider
    )
    {
        _appDbContext = appDbContext;
        _options = options;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SetupTokenValidationResult> Validate(
        Guid setupToken,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext.UserOnboardingRecords.FindAsync(
            [setupToken],
            cancellationToken: cancellationToken
        );

        if (userRecord is null)
        {
            return SetupTokenValidationResult.Err(new SetupTokenValidationError.DoesNotExist());
        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        if (userRecord.HasExpired(_dateTimeProvider.GetUtcNow(), timeSpan))
        {
            return SetupTokenValidationResult.Err(new SetupTokenValidationError.Expired());
        }

        return SetupTokenValidationResult.Ok();
    }
}
