namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Builds the form models when the API starts, so invalid form attributes stop it starting
/// instead of failing the first request.
/// </summary>
internal sealed class FormModelStartupCheck(FormModelRegistry registry) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolving the registry is the check: its factory throws FormModelException.
        GC.KeepAlive(registry);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
