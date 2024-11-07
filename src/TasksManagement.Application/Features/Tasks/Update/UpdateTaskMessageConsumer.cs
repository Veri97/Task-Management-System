using Microsoft.Extensions.Options;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Events;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;

namespace TasksManagement.Application.Features.Tasks.Update;

public class UpdateTaskMessageConsumer : IMessageConsumer<UpdateTaskMessage>
{
    private readonly ITasksRepository _tasksRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceBusHandler _serviceBusHandler;
    private readonly RabbitMQSettings _rabbitMQSettings;

    public UpdateTaskMessageConsumer(
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

    public async Task ProcessMessageAsync(UpdateTaskMessage message)
    {
        var task = await _tasksRepository.GetTaskById(message!.Id);

        task!.Status = message.NewStatus;

        await _unitOfWork.SaveChangesAsync();

        await _serviceBusHandler.SendMessage(_rabbitMQSettings.ActionCompletedQueueName,
            new ActionCompletedEvent
            {
                Message = $"Task with id ({task.Id}) was updated successfully!"
            });
    }
}
