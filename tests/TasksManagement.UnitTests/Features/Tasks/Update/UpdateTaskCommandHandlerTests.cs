using FluentAssertions;
using MediatR;
using Moq;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Features.Tasks;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Update;

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateTaskCommandHandler(_tasksRepositoryMock.Object, _unitOfWorkMock.Object);
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

        _tasksRepositoryMock.Setup(x =>
              x.GetTaskById(command.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(() => null);

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

        var mockedTask = new TaskEntity
        {
            Id = 1,
            Name = "Task 1",
            Status = Core.Enums.Status.InProgress,
            Description = "Test description",
            AssignedTo = "user1@example.com"
        };

        _tasksRepositoryMock.Setup(x =>
             x.GetTaskById(command.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(mockedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        mockedTask.Status.Should().Be(command.NewStatus);

        _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
