using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskRequest
{
    public Status NewStatus { get; set; }
}
