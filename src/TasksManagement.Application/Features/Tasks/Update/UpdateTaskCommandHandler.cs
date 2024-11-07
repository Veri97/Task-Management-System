using MediatR;
using Microsoft.Extensions.Options;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Unit>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IServiceBusHandler _serviceBusHandler;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public UpdateTaskCommandHandler(
        ITasksRepository tasksRepository, 
        IServiceBusHandler serviceBusHandler,
        IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _tasksRepository = tasksRepository;
        _serviceBusHandler = serviceBusHandler;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public async Task<Unit> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskExists = await _tasksRepository.Exists(request.Id, cancellationToken);

        if (!taskExists)
        {
            throw new NotFoundException($"Task with id {request.Id} does not exist");
        }

        await _serviceBusHandler.SendMessage(_rabbitMQSettings.TaskUpdateQueueName, 
            new UpdateTaskMessage
            {
                Id = request.Id,
                NewStatus = request.NewStatus
            });

        return Unit.Value;
    }
}
