using MediatR;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public Status NewStatus { get; set; }
}