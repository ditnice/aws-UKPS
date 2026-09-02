using System.Text.Json;

namespace UKPS.Api.Application.InternalServices.Communication;

internal sealed class EmailQueueMessageHandler
{
    private readonly SesEmailService _emailService;

    public EmailQueueMessageHandler(SesEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleEmailQueueMessage(
        string messageBody,
        CancellationToken cancellationToken
    )
    {
        var command = JsonSerializer.Deserialize<SendEmailCommand>(messageBody);
        if (command != null)
        {
            await _emailService.SendEmail(command, cancellationToken);
        }
    }
}
