namespace UKPS.Api.WebApi;

internal static class AwsConfigurationExtensions
{
    public static void ConfigureAwsSecrets(this ConfigurationManager configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        bool loadSecrets = configuration.GetValue<bool>("AWS:LoadSecrets");
        if (!loadSecrets)
        {
            return;
        }

        configuration.AddSystemsManager(
            "/aws/reference/secretsmanager/ukps/dev/ukps-service/cognito-client"
        );
        configuration["Cognito:ClientSecret"] = configuration["ClientSecret"];
    }
}
