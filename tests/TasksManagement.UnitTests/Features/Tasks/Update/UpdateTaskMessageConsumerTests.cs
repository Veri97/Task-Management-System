using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Events;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Update;

public class UpdateTaskMessageConsumerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IServiceBusHandler> _serviceBusHandlerMock;
    private readonly Mock<IOptions<RabbitMQSettings>> _rabbitMQSettingsMock;
    private readonly UpdateTaskMessageConsumer _consumer;

    public UpdateTaskMessageConsumerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _serviceBusHandlerMock = new Mock<IServiceBusHandler>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _rabbitMQSettingsMock = new Mock<IOptions<RabbitMQSettings>>();
        _rabbitMQSettingsMock.SetupGet(x => x.Value).Returns(new RabbitMQSettings
        {
            ActionCompletedQueueName = "task_action_completed_test_queue"
        });

        _consumer = new UpdateTaskMessageConsumer(
            _tasksRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _serviceBusHandlerMock.Object,
            _rabbitMQSettingsMock.Object);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldUpdateTaskStatusAndPublishActionCompletedEvent_WhenMessageIsReceived()
    {
        // Arrange
        var message = new UpdateTaskMessage
        {
            Id = 1,
            NewStatus = Status.Completed
        };

        var existingTask = new TaskEntity
        {
            Id = 1,
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.InProgress,
            AssignedTo = "user1@example.com"
        };

        _tasksRepositoryMock.Setup(x => x.GetTaskById(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act
        await _consumer.ProcessMessageAsync(message);

        // Assert
        existingTask.Status.Should().Be(message.NewStatus);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _serviceBusHandlerMock.Verify(
               x => x.SendMessage(
                   _rabbitMQSettingsMock.Object.Value.ActionCompletedQueueName,
                   It.Is<ActionCompletedEvent>(
                       t => string.Equals(t.Message, $"Task with id ({existingTask.Id}) was updated successfully!")
                   )), Times.Once);
    }
}
