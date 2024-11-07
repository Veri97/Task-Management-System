using TasksManagement.Application.Abstractions;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskMessage : IMessage
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Status Status { get; set; }
    public string? AssignedTo { get; set; }
}
