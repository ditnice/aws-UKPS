using System.Diagnostics;
using Microsoft.Extensions.Options;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Application.InternalServices.Temporal;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.Identity;
using SetupTokenValidationResult = UKPS.Api.Application.Common.Result<UKPS.Api.Application.AuthorisationAdministration.SetupTokenValidationError>;

namespace UKPS.Api.Application.AuthorisationAdministration;

internal class AuthorisationAdministrationService : IAuthorisationAdministrationService
{
    private readonly AppDbContext _appDbContext;
    private readonly CognitoWebIdentityAdministrationService _webIdentityAdministrationService;
    private readonly IOptions<UserOnboardingConfiguration> _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthorisationAdministrationService(
        AppDbContext appDbContext,
        CognitoWebIdentityAdministrationService webIdentityAdministrationService,
        IOptions<UserOnboardingConfiguration> options,
        IDateTimeProvider dateTimeProvider
    )
    {
        _appDbContext = appDbContext;
        _webIdentityAdministrationService = webIdentityAdministrationService;
        _options = options;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<UserSetupError>> SetupUser(
        SetupUserCommand command,
        CancellationToken cancellationToken
    )
    {
        UserOnboardingRecord? userRecord = await _appDbContext.UserOnboardingRecords.FindAsync(
            [command.SetupToken],
            cancellationToken: cancellationToken
        );

        if (userRecord is null)
        {
            return Result<UserSetupError>.Err(new UserSetupError.DoesNotExist());
        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        SetupTokenState? state = userRecord.GetCurrentState(
            _dateTimeProvider.GetUtcNow(),
            timeSpan
        );

        if (state != SetupTokenState.Valid)
        {
            return state switch
            {
                SetupTokenState.Consumed => Result<UserSetupError>.Err(
                    new UserSetupError.Consumed()
                ),
                SetupTokenState.Expired => Result<UserSetupError>.Err(new UserSetupError.Expired()),
                _ => throw new UnreachableException(
                    $"Unexpected setup token state '{state}' encountered when validating setup token."
                ),
            };
        }

        userRecord.MarkAsConsumed(_dateTimeProvider.GetUtcNow());

        await _appDbContext.SaveChangesAsync(cancellationToken);

        Result<UpdatePasswordError> result = await _webIdentityAdministrationService.UpdatePassword(
            userRecord.UserEmail,
            command.NewPassword,
            cancellationToken
        );

        if (result.IsErr)
        {
            UserSetupError error = result.Error.Match(
                invalidPassword: _ => new UserSetupError.InvalidPassword()
            );
            return Result<UserSetupError>.Err(error);
        }

        return Result<UserSetupError>.Ok();
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

        TimeSpan timeSpan = TimeSpan.FromSeconds(_options.Value.SetupTokenExpiryTimeSeconds);
        return (userRecord?.GetCurrentState(_dateTimeProvider.GetUtcNow(), timeSpan)) switch
        {
            null => SetupTokenValidationResult.Err(new SetupTokenValidationError.DoesNotExist()),
            SetupTokenState.Expired => SetupTokenValidationResult.Err(
                new SetupTokenValidationError.Expired()
            ),
            SetupTokenState.Consumed => SetupTokenValidationResult.Err(
                new SetupTokenValidationError.Consumed()
            ),
            _ => SetupTokenValidationResult.Ok(),
        };
    }
}
