namespace TasksManagement.Application.Abstractions;

public interface IServiceBusHandler
{
    Task SendMessage<T>(string queueName, T message)
        where T : IMessage;

    void ReceiveMessage<T>(string queueName)
        where T : IMessage;
}