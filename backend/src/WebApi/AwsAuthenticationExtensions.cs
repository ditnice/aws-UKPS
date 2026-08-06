using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using UKPS.Api.Application.Authentication;
using UKPS.Api.WebApi.InternalServices.Authentication;

namespace UKPS.Api.WebApi;

internal static class AwsAuthenticationExtensions
{
    public static void AddAwsBearerAuthentication(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        DevAuthenticationConfiguration devAuthenticationConfiguration =
            builder
                .Configuration.GetSection(DevAuthenticationConfiguration.SectionName)
                .Get<DevAuthenticationConfiguration>()
            ?? new DevAuthenticationConfiguration();

        if (devAuthenticationConfiguration.IsEnabled)
        {
            ConfigureDevAuthentication(builder.Services);
            return;
        }

        ConfigureStandardAuthentication(builder);
    }

    private static void ConfigureStandardAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.TryAddTransient<ITokenValidationHandler, TokenValidationHandler>();
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                CognitoConfiguration configuration = RetrieveConfiguration(builder);
                var authority = new Uri(configuration.ServiceUrl, configuration.UserPoolId);
                options.Authority = authority.AbsoluteUri;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = authority.AbsoluteUri,
                    ValidateIssuerSigningKey = true,

                    // Amazon Cognito access tokens do not contain an `aud` claim. Instead, the
                    // Cognito app client is identified by the `client_id` claim. Consequently,
                    // ValidateAudience cannot be used to validate Cognito access tokens in the
                    // same way it can be used for ID tokens. The client ID is validated separately
                    // when the token is validated.
#pragma warning disable CA5404 // Do not disable token validation checks
                    ValidateAudience = false,
#pragma warning restore CA5404 // Do not disable token validation checks
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = HandleOnMessageReceived,
                    OnTokenValidated = ctx => HandleOnTokenValidated(ctx),
                };
            });
    }

    private static void ConfigureDevAuthentication(IServiceCollection services)
    {
        var authOptions = new DevAuthenticationOptions();
        services.AddSingleton(authOptions);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = DevAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = DevAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(
                DevAuthHandler.AuthenticationScheme,
                _ => { }
            );
    }

    private static CognitoConfiguration RetrieveConfiguration(WebApplicationBuilder builder)
    {
        return builder
                .Configuration.GetSection(CognitoConfiguration.SectionName)
                .Get<CognitoConfiguration>()
            ?? throw new InvalidOperationException(
                $"Jwt configuration section [{CognitoConfiguration.SectionName}] is missing or invalid."
            );
    }

    private static Task HandleOnMessageReceived(MessageReceivedContext context)
    {
        context.Token = context.Request.Cookies["access_token"];
        return Task.CompletedTask;
    }

    private static Task HandleOnTokenValidated(TokenValidatedContext context)
    {
        var handler =
            context.HttpContext.RequestServices.GetRequiredService<ITokenValidationHandler>();
        return handler.Handle(context, context.HttpContext.RequestAborted);
    }
}
