using FluentAssertions;
using MediatR;
using Moq;
using TasksManagement.Application.Exceptions;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.Create;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateTaskCommandHandler(_tasksRepositoryMock.Object, _unitOfWorkMock.Object);
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

        _tasksRepositoryMock.Setup(x =>
              x.TaskByNameExists(command.Name, It.IsAny<CancellationToken>()))
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

        _tasksRepositoryMock.Setup(x =>
             x.TaskByNameExists(command.Name, It.IsAny<CancellationToken>()))
           .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _tasksRepositoryMock.Verify(
                x => x.Create(
                    It.Is<TaskEntity>(
                        t => t.Name == command.Name &&
                             t.Description == command.Description &&
                             t.Status == command.Status.Value &&
                             t.AssignedTo == command.AssignedTo
                    ),
                    It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
