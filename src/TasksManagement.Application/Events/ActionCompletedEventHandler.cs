using Microsoft.Extensions.Logging;
using TasksManagement.Application.Abstractions;

namespace TasksManagement.Application.Events;

public class ActionCompletedEventHandler : IMessageConsumer<ActionCompletedEvent>
{
    private readonly ILogger<ActionCompletedEventHandler> _logger;

    public ActionCompletedEventHandler(ILogger<ActionCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task ProcessMessageAsync(ActionCompletedEvent message)
    {
        _logger.LogInformation(message.Message);
        return Task.CompletedTask;
    }
}
