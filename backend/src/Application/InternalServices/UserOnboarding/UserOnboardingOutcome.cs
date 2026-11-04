using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.UserOnboarding;

internal sealed record UserOnboardingOutcome(User User, bool IsNewUser);
