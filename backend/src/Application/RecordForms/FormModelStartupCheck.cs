using UKPS.Api.Application.RecordForms.CrossPageRules;

namespace UKPS.Api.Application.RecordForms;

/// <summary>
/// Builds the form models and cross-page rules when the API starts, so invalid form attributes or
/// rules stop it starting instead of failing the first request.
/// </summary>
internal sealed class FormModelStartupCheck(
    FormModelRegistry registry,
    CrossPageRuleRegistry crossPageRules
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Resolving the registries is the check: their factories throw FormModelException.
        GC.KeepAlive(registry);
        GC.KeepAlive(crossPageRules);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
