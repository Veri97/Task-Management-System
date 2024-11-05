using MediatR;
using TasksManagement.Application.Exceptions;
using TasksManagement.Core.Contracts;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Unit>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(ITasksRepository tasksRepository, IUnitOfWork unitOfWork)
    {
        _tasksRepository = tasksRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasksRepository.GetTaskById(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException($"Task with id {request.Id} does not exist");
        }

        task.Status = request.NewStatus;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
