using Amazon.SimpleEmailV2;

namespace UKPS.Api.Application.InternalServices.Communication;

internal static class CommunicationDependencyInjectionManager
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddAWSService<IAmazonSimpleEmailServiceV2>();
        services.AddScoped<IEmailService, SesEmailService>();
        return services;
    }
}
