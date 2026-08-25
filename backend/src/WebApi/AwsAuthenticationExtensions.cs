using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        DevAuthenticationOptions devAuthenticationConfiguration =
            builder
                .Configuration.GetSection(DevAuthenticationOptions.SectionName)
                .Get<DevAuthenticationOptions>()
            ?? new DevAuthenticationOptions();

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
                Uri authority = RetrieveAuthority(builder);

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
        var authOptions = new DevAuthenticationClaims();
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

    private static Uri RetrieveAuthority(WebApplicationBuilder builder)
    {
        try
        {
            var configuration =
                builder.Configuration.GetSection(CognitoOptions.SectionName).Get<CognitoOptions>()
                ?? throw new InvalidOperationException(
                    $"Jwt configuration section [{CognitoOptions.SectionName}] is missing or invalid."
                );
            return new Uri(configuration.ServiceUrl, configuration.UserPoolId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Jwt configuration section [{CognitoOptions.SectionName}] is missing or invalid.",
                ex
            );
        }
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
