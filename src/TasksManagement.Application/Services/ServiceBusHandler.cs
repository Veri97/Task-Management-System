using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Settings;

namespace TasksManagement.Application.Services;

public class ServiceBusHandler : IServiceBusHandler
{
    private readonly IRabbitMQClient _rabbitMQClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusHandler> _logger;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public ServiceBusHandler(
        IServiceProvider serviceProvider, 
        IRabbitMQClient rabbitMQClient,
        ILogger<ServiceBusHandler> logger,
        IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _serviceProvider = serviceProvider;
        _rabbitMQClient = rabbitMQClient;
        _logger = logger;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public void ReceiveMessage<T>(string queueName)
        where T : IMessage
    {
        var channel = _rabbitMQClient.CreateChannel();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var messageAsString = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Received '{typeof(T).Name}' from '{queueName}' queue");

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_rabbitMQSettings.RetryCount, 
                   attempt => TimeSpan.FromSeconds(_rabbitMQSettings.RetryDurationInSeconds), (exception, timeSpan, attempt, context) =>
                   {
                      _logger.LogError(exception, $"An exception occurred during message processing: {exception.Message}. Retrying attempt {attempt}...");
                   });

            await policy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageConsumer<T>>();

                var message = JsonSerializer.Deserialize<T>(messageAsString);
                await messageHandler.ProcessMessageAsync(message!);
            });
        };

        channel.BasicConsume(queue: queueName,
                             autoAck: true,
                             consumer: consumer);
    }
    public Task SendMessage<T>(string queueName, T message)
        where T : IMessage
    {
        var channel = _rabbitMQClient.CreateChannel();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(_rabbitMQSettings.RetryCount, 
                attempt => TimeSpan.FromSeconds(_rabbitMQSettings.RetryDurationInSeconds), (exception, timeSpan, attempt, context) =>
                {
                    _logger.LogError(exception, $"An exception occurred during message publishing: {exception.Message}. Retrying attempt {attempt}...");
                });

        policy.Execute(() =>
        {
            channel.BasicPublish(exchange: _rabbitMQSettings.ExchangeName,
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"Sent '{typeof(T).Name}' to '{queueName}' queue");
        });

        return Task.CompletedTask;
    }
}
