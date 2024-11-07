using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Settings;

namespace TasksManagement.Infrastructure.MessageBroker;

public class RabbitMQClient : IRabbitMQClient, IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly RabbitMQSettings _rabbitMQSettings;
    public IModel Channel { get; private set; }
    private IConnection _connection;
    
    public RabbitMQClient(IConnectionFactory connectionFactory, IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _connectionFactory = connectionFactory;
        _rabbitMQSettings = rabbitMQSettings.Value;
        CreateChannel();
    }

    public void Publish(string exchange, string routingKey, IBasicProperties? basicProperties, byte[] body)
    {
        Channel.BasicPublish(exchange, routingKey, basicProperties, body);
    }

    public void Consume(string queue, bool autoAck, IBasicConsumer consumer)
    {
        Channel.BasicConsume(queue, autoAck, consumer);
    }

    private void CreateChannel()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = _connectionFactory.CreateConnection();
        }

        if (Channel == null || !Channel.IsOpen)
        {
            Channel = _connection.CreateModel();

            Channel.ExchangeDeclare(
                exchange: _rabbitMQSettings.ExchangeName,
                type: _rabbitMQSettings.ExchangeType,
                durable: true,
                autoDelete: false);

            DeclareQueues();

            BindQueues();
        }
    }

    private void DeclareQueues()
    {
        Channel.QueueDeclare(
                queue: _rabbitMQSettings.TaskCreateQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);

        Channel.QueueDeclare(
                queue: _rabbitMQSettings.TaskUpdateQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);

        Channel.QueueDeclare(
                queue: _rabbitMQSettings.ActionCompletedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                null);
    }

    private void BindQueues()
    {
        Channel.QueueBind(
                queue: _rabbitMQSettings.TaskCreateQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.TaskCreateQueueName);

        Channel.QueueBind(
                queue: _rabbitMQSettings.TaskUpdateQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.TaskUpdateQueueName);

        Channel.QueueBind(
                queue: _rabbitMQSettings.ActionCompletedQueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.ActionCompletedQueueName);

    }

    public void Dispose()
    {
        Channel?.Close();
        Channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();
    }
}
