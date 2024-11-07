using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Settings;

namespace TasksManagement.Infrastructure.MessageBroker;

public class RabbitMQClient : IRabbitMQClient, IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private IModel _channel;
    private IConnection _connection;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public RabbitMQClient(IConnectionFactory connectionFactory, IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _connectionFactory = connectionFactory;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public IModel CreateChannel()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        if (_channel == null || !_channel.IsOpen)
        {
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _rabbitMQSettings.ExchangeName,
                type: _rabbitMQSettings.ExchangeType,
                durable: true,
                autoDelete: false);

            DeclareQueues();

            BindQueues();
        }

        return _channel;
    }

    private void DeclareQueues()
    {
        _channel.QueueDeclare(
                queue: _rabbitMQSettings.TaskCreateQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);

        _channel.QueueDeclare(
                queue: _rabbitMQSettings.TaskUpdateQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);

        _channel.QueueDeclare(
                queue: _rabbitMQSettings.ActionCompletedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);
    }

    private void BindQueues()
    {
        _channel.QueueBind(
                queue: _rabbitMQSettings.TaskCreateQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.TaskCreateQueueName);

        _channel.QueueBind(
                queue: _rabbitMQSettings.TaskUpdateQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.TaskUpdateQueueName);

        _channel.QueueBind(
                queue: _rabbitMQSettings.ActionCompletedQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.ActionCompletedQueueName);

    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();
    }
}
