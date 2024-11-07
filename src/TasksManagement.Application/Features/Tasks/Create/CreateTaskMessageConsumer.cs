using Microsoft.Extensions.Options;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Events;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;

namespace TasksManagement.Application.Features.Tasks.Create;

public class CreateTaskMessageConsumer : IMessageConsumer<CreateTaskMessage>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceBusHandler _serviceBusHandler;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public CreateTaskMessageConsumer(
        ITasksRepository tasksRepository,
        IUnitOfWork unitOfWork, 
        IServiceBusHandler serviceBusHandler,
        IOptions<RabbitMQSettings> rabbitMQSettings)
    {
        _tasksRepository = tasksRepository;
        _unitOfWork = unitOfWork;
        _serviceBusHandler = serviceBusHandler;
        _rabbitMQSettings = rabbitMQSettings.Value;
    }

    public async Task ProcessMessageAsync(CreateTaskMessage message)
    {
        await _tasksRepository.Create(new TaskEntity
        {
            Name = message.Name,
            Description = message.Description,
            Status = message.Status,
            AssignedTo = message.AssignedTo
        });

        await _unitOfWork.SaveChangesAsync();

        await _serviceBusHandler.SendMessage(_rabbitMQSettings.ActionCompletedQueueName,
            new ActionCompletedEvent
            {
                Message = $"Task with name '{message.Name}' was created successfully!"
            });
    }
}
