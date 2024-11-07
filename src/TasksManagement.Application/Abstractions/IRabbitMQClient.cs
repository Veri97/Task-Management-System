using RabbitMQ.Client;

namespace TasksManagement.Application.Abstractions;

public interface IRabbitMQClient
{
    IModel Channel { get; }

    void Publish(
        string exchange, 
        string routingKey, 
        IBasicProperties? basicProperties, 
        byte[] body);

    void Consume(
        string queue,
        bool autoAck,
        IBasicConsumer consumer);
}