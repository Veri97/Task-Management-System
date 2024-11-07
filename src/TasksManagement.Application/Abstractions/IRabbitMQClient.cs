using RabbitMQ.Client;

namespace TasksManagement.Application.Abstractions;

public interface IRabbitMQClient
{
    IModel CreateChannel();
}