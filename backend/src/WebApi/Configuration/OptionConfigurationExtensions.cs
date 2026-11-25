using Microsoft.Extensions.Options;
using UKPS.Api.WebApi.OpenApi;
using UKPS.Api.WebApi.Validators;

namespace UKPS.Api.WebApi.Configuration;

/// <summary>
/// Extension methods for registering strongly typed application options
/// with the <see cref="WebApplicationBuilder"/>.
/// </summary>
internal static class OptionConfigurationExtensions
{
    /// <summary>
    /// Binds the options type <typeparamref name="T"/> to the configuration section named by
    /// <see cref="IAppOptions.SectionName"/>. It also turns on data annotation validation and
    /// runs that validation when the application starts.
    /// </summary>
    /// <typeparam name="T">
    /// The options type to bind. It must be a reference type that implements
    /// <see cref="IAppOptions"/>, which supplies the configuration section name.
    /// </typeparam>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to register the options with.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance, so further calls can be chained.</returns>
    /// <remarks>
    /// Validation uses the <see cref="System.ComponentModel.DataAnnotations"/> attributes on
    /// <typeparamref name="T"/>. Because the options are validated on start, invalid or missing
    /// configuration makes the application fail at startup with an
    /// <see cref="Microsoft.Extensions.Options.OptionsValidationException"/>. Without this, the
    /// error would only appear the first time the options are used.
    /// <para>
    /// Validation is skipped while the build generates the OpenAPI documents (see
    /// <see cref="OpenApiDocumentGeneration"/>), because placeholder values such as
    /// <see cref="RequiredConfigurationAttribute.FromEnvironmentVariable"/> are not replaced then,
    /// and hosted services that read options still start during generation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.BindAppOptions&lt;DatabaseOptions&gt;();
    /// </code>
    /// </example>
    public static WebApplicationBuilder BindAppOptions<T>(this WebApplicationBuilder builder)
        where T : class, IAppOptions
    {
        OptionsBuilder<T> options = builder
            .Services.AddOptions<T>()
            .Bind(builder.Configuration.GetSection(T.SectionName));

        if (!OpenApiDocumentGeneration.IsRunning)
        {
            options.ValidateDataAnnotations().ValidateOnStart();
        }

        return builder;
    }
}
