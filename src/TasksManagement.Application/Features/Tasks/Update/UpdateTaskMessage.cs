using TasksManagement.Application.Abstractions;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskMessage : IMessage
{
    public int Id { get; set; }
    public Status NewStatus { get; set; }
}
