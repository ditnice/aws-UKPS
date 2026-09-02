namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed class EmailQueueListener : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EmailQueueListener(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailQueueProcessor =
                scope.ServiceProvider.GetRequiredService<EmailQueueProcessor>();
            await emailQueueProcessor.ProcessEmailQueue(stoppingToken);
        }
    }
}
