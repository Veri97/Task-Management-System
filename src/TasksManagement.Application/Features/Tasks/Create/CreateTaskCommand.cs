using MediatR;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskCommand : IRequest<Unit>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Status? Status { get; set; }
    public string? AssignedTo { get; set; }
}
