using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Update;

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IServiceBusHandler> _serviceBusHandlerMock;
    private readonly Mock<IOptions<RabbitMQSettings>> _rabbitMQSettingsMock;
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _serviceBusHandlerMock = new Mock<IServiceBusHandler>();

        _rabbitMQSettingsMock = new Mock<IOptions<RabbitMQSettings>>();
        _rabbitMQSettingsMock.SetupGet(x => x.Value).Returns(new RabbitMQSettings
        {
            TaskUpdateQueueName = "task_update_test_queue"
        });

        _handler = new UpdateTaskCommandHandler(
            _tasksRepositoryMock.Object,
            _serviceBusHandlerMock.Object,
            _rabbitMQSettingsMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            Id = 1,
            NewStatus = Status.InProgress
        };

        _tasksRepositoryMock.Setup(x => x.Exists(command.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        // Act
        Func<Task<Unit>> result = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<NotFoundException>().WithMessage($"Task with id {command.Id} does not exist");
    }

    [Fact]
    public async Task Handle_ShouldSuccessfullyUpdateTaskStatus_WhenTaskExists()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            Id = 1,
            NewStatus = Status.Completed
        };

        _tasksRepositoryMock.Setup(x => x.Exists(command.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _serviceBusHandlerMock.Verify(
                x => x.SendMessage(
                    _rabbitMQSettingsMock.Object.Value.TaskUpdateQueueName,
                    It.Is<UpdateTaskMessage>(
                        t => t.Id == command.Id && t.NewStatus == command.NewStatus
                    )), Times.Once);
    }
}
