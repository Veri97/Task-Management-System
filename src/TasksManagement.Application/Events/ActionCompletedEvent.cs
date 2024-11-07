using TasksManagement.Application.Abstractions;

namespace TasksManagement.Application.Events;

public class ActionCompletedEvent : IMessage
{
    public string Message { get; set; } = null!;
}
