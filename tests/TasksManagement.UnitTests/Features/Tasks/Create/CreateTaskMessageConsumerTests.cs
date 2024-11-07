using Microsoft.Extensions.Options;
using Moq;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Events;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Create;

public class CreateTaskMessageConsumerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IServiceBusHandler> _serviceBusHandlerMock;
    private readonly Mock<IOptions<RabbitMQSettings>> _rabbitMQSettingsMock;
    private readonly CreateTaskMessageConsumer _consumer;

    public CreateTaskMessageConsumerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _serviceBusHandlerMock = new Mock<IServiceBusHandler>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _rabbitMQSettingsMock = new Mock<IOptions<RabbitMQSettings>>();
        _rabbitMQSettingsMock.SetupGet(x => x.Value).Returns(new RabbitMQSettings
        {
            ActionCompletedQueueName = "task_action_completed_test_queue"
        });

        _consumer = new CreateTaskMessageConsumer(
            _tasksRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _serviceBusHandlerMock.Object,
            _rabbitMQSettingsMock.Object);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldCreateNewTaskAndPublishActionCompletedEvent_WhenMessageIsReceived()
    {
        // Arrange
        var message = new CreateTaskMessage
        {
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.InProgress,
            AssignedTo = "user1@example.com"
        };

        // Act
        await _consumer.ProcessMessageAsync(message);

        // Assert
        _tasksRepositoryMock.Verify(x =>
             x.Create(
                 It.Is<TaskEntity>(t => t.Name == message.Name &&
                                        t.Description == message.Description &&
                                        t.Status == message.Status &&
                                        t.AssignedTo == message.AssignedTo),
                 It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _serviceBusHandlerMock.Verify(
               x => x.SendMessage(
                   _rabbitMQSettingsMock.Object.Value.ActionCompletedQueueName,
                   It.Is<ActionCompletedEvent>(
                       t => string.Equals(t.Message, $"Task with name '{message.Name}' was created successfully!")
                   )), Times.Once);
    }
}
