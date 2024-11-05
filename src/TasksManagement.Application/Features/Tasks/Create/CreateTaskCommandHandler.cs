using MediatR;
using TasksManagement.Application.Exceptions;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Unit>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(ITasksRepository tasksRepository, IUnitOfWork unitOfWork)
    {
        _tasksRepository = tasksRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (await _tasksRepository.TaskByNameExists(request.Name, cancellationToken))
        {
            throw new ConflictException("A task with the same name already exists");
        }

        var task = new TaskEntity
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status.HasValue ? request.Status.Value : Status.NotStarted,
            AssignedTo = request.AssignedTo
        };

        await _tasksRepository.Create(task, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
