using Amazon.SimpleEmailV2;
using Amazon.SQS;

namespace UKPS.Api.Application.InternalServices.Communication;

internal static class CommunicationDependencyInjectionManager
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddHostedService<EmailQueueListener>();
        services.AddScoped<EmailQueueProcessor>();
        services.AddScoped<EmailQueueMessageHandler>();
        services.AddAWSService<IAmazonSQS>();
        services.AddAWSService<IAmazonSimpleEmailServiceV2>();
        services.AddScoped<SesEmailService>();
        services.AddScoped<IEmailService, QueuedEmailService>();
        return services;
    }
}
