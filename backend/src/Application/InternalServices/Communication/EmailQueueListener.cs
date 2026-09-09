namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed partial class EmailQueueListener : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailQueueListener> _logger;

    public EmailQueueListener(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EmailQueueListener> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var emailQueueProcessor =
                    scope.ServiceProvider.GetRequiredService<EmailQueueProcessor>();
                await emailQueueProcessor.ProcessEmailQueue(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogEmailListeningFailure(ex);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Avoid spamming if an exception occurs
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occurred whilst listening for email messages."
    )]
    private partial void LogEmailListeningFailure(Exception ex);
}
