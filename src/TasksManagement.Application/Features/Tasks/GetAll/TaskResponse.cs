using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.GetAll;

public class TaskResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Status Status { get; set; }
    public string? AssignedTo { get; set; }
}
