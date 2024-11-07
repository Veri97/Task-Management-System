using Microsoft.Extensions.Options;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Events;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Application.Settings;

namespace TasksManagement.Api.HostedServices;

public class RabbitMQServiceBusHosted : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public RabbitMQServiceBusHosted(IServiceProvider serviceProvider, IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _serviceProvider = serviceProvider;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var serviceBusHandler = scope.ServiceProvider.GetRequiredService<IServiceBusHandler>();

        serviceBusHandler.ReceiveMessage<CreateTaskMessage>(_rabbitMQSettings.TaskCreateQueueName);
        serviceBusHandler.ReceiveMessage<UpdateTaskMessage>(_rabbitMQSettings.TaskUpdateQueueName);
        serviceBusHandler.ReceiveMessage<ActionCompletedEvent>(_rabbitMQSettings.ActionCompletedQueueName);

        return Task.CompletedTask;
    }
}