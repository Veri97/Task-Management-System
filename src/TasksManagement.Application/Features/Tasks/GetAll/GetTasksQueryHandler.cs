using MediatR;
using TasksManagement.Core.Contracts;

namespace TasksManagement.Application.Features.Tasks.GetAll;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskResponse>>
{
    private readonly ITasksRepository _tasksRepository;

    public GetTasksQueryHandler(ITasksRepository tasksRepository)
    {
         _tasksRepository = tasksRepository;
    }

    public async Task<List<TaskResponse>> Handle(GetTasksQuery request,
        CancellationToken cancellationToken)
    {
        var tasks = await _tasksRepository.GetAllTasks(cancellationToken);

        return tasks.Select(x => new TaskResponse
        { 
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Status = x.Status,
            AssignedTo = x.AssignedTo,
        }).ToList();
    }
}
