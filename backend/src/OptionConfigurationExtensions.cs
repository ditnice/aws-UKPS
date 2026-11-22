namespace UKPS.Api;

internal static class OptionConfigurationExtensions
{
    public static WebApplicationBuilder BindAppOptions<T>(this WebApplicationBuilder builder)
        where T : class, IAppOptions
    {
        builder
            .Services.AddOptions<T>()
            .Bind(builder.Configuration.GetSection(T.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }
}
