using MediatR;

namespace TasksManagement.Application.Features.Tasks.GetAll;

public class GetTasksQuery : IRequest<List<TaskResponse>> { }
