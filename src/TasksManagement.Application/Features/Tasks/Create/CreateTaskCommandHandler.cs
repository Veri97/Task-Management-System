using MediatR;
using Microsoft.Extensions.Options;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Enums;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Unit>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IServiceBusHandler _serviceBusHandler;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public CreateTaskCommandHandler(
        ITasksRepository tasksRepository, 
        IServiceBusHandler serviceBusHandler,
        IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _tasksRepository = tasksRepository;
        _serviceBusHandler = serviceBusHandler;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public async Task<Unit> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (await _tasksRepository.Exists(request.Name, cancellationToken))
        {
            throw new ConflictException("A task with the same name already exists");
        }

        await _serviceBusHandler.SendMessage(_rabbitMQSettings.TaskCreateQueueName, 
            new CreateTaskMessage
            {
                Name = request.Name,
                Description = request.Description,
                Status = request.Status.HasValue ? request.Status.Value : Status.NotStarted,
                AssignedTo = request.AssignedTo
            });
 
        return Unit.Value;
    }
}
