using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Create;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IServiceBusHandler> _serviceBusHandlerMock;
    private readonly Mock<IOptions<RabbitMQSettings>> _rabbitMQSettingsMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _serviceBusHandlerMock = new Mock<IServiceBusHandler>();

        _rabbitMQSettingsMock = new Mock<IOptions<RabbitMQSettings>>();
        _rabbitMQSettingsMock.SetupGet(x => x.Value).Returns(new RabbitMQSettings
        {
            TaskCreateQueueName = "task_create_test_queue"
        });

        _handler = new CreateTaskCommandHandler(
            _tasksRepositoryMock.Object, 
            _serviceBusHandlerMock.Object,
            _rabbitMQSettingsMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenAnotherTaskWithTheSameNameExists()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        _tasksRepositoryMock.Setup(x => x.Exists(command.Name, It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);

        // Act
        Func<Task<Unit>> result = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<ConflictException>().WithMessage("A task with the same name already exists");
    }

    [Fact]
    public async Task Handle_ShouldSuccessfullyCreateTask_WhenTaskIsUniqueByName()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        _tasksRepositoryMock.Setup(x => x.Exists(command.Name, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _serviceBusHandlerMock.Verify(
                x => x.SendMessage(
                    _rabbitMQSettingsMock.Object.Value.TaskCreateQueueName,
                    It.Is<CreateTaskMessage>(
                        t => t.Name == command.Name &&
                             t.Description == command.Description &&
                             t.Status == command.Status.Value &&
                             t.AssignedTo == command.AssignedTo
                    )), Times.Once);
    }
}
