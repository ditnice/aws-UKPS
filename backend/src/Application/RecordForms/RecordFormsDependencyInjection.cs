using Microsoft.Extensions.DependencyInjection.Extensions;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Application.RecordForms.Scanning;

namespace UKPS.Api.Application.RecordForms;

internal static class RecordFormsDependencyInjection
{
    public static IServiceCollection AddRecordFormServices(this IServiceCollection services)
    {
        services.TryAddSingleton(static _ => OptionProviderRegistry.CreateDefault());
        services.TryAddSingleton(static sp =>
            FormModelFactory.Create(sp.GetRequiredService<OptionProviderRegistry>())
        );
        services.AddHostedService<FormModelStartupCheck>();
        services.TryAddScoped<RecordFormPageLoader>();
        services.TryAddScoped<IRecordFormQueryService, RecordFormQueryService>();
        services.TryAddScoped<IRecordFormSaveService, RecordFormSaveService>();
        return services;
    }
}
