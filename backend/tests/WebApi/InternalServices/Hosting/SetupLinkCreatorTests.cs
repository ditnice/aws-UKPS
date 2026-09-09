using Microsoft.Extensions.Options;
using Shouldly;
using UKPS.Api.Application.Authentication;
using UKPS.Api.WebApi.InternalServices.Hosting;

namespace UKPS.Api.Tests.WebApi.InternalServices.Hosting;

public class SetupLinkCreatorTests
{
    [Fact]
    public void GetSetupLink_ShouldReturnFrontendInitiateAuthLinkWithSetupToken()
    {
        var setupToken = Guid.Parse("48b5becd-f98c-4897-98aa-be37eecb6a68");
        var creator = new SetupLinkCreator(
            Options.Create(
                new UserOnboardingOptions { SetupLink = new Uri("https://frontend.example") }
            )
        );

        Uri setupLink = creator.GetSetupLink(setupToken);

        setupLink.ShouldBe(
            new Uri(
                "https://frontend.example/auth/sign-up/initiate?setupToken=48b5becd-f98c-4897-98aa-be37eecb6a68"
            )
        );
    }
}
