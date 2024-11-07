namespace TasksManagement.Application.Abstractions;

public interface IMessageConsumer<T>
    where T : IMessage
{
    Task ProcessMessageAsync(T message);
}