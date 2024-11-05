using FluentAssertions;
using Moq;
using TasksManagement.Application.Features.Tasks.GetAll;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;

namespace TasksManagement.UnitTests.Features.Tasks.GetAll;

public class GetTasksQueryHandlerTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock;
    private readonly GetTasksQueryHandler _handler;

    public GetTasksQueryHandlerTests()
    {
        _tasksRepositoryMock = new Mock<ITasksRepository>();
        _handler = new GetTasksQueryHandler(_tasksRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnListOfTasks_WhenTasksExist()
    {
        // Arrange
        var query = new GetTasksQuery();

        _tasksRepositoryMock.Setup(x => x.GetAllTasks(It.IsAny<CancellationToken>()))
           .ReturnsAsync(GetMockedTasks());

        // Act
        List<TaskResponse> tasks = await _handler.Handle(query, CancellationToken.None);

        // Assert
        VerifyTaskListResponse(tasks);
    }

    private void VerifyTaskListResponse(List<TaskResponse> response)
    {
        response.Should().NotBeEmpty();
        response.Should().BeEquivalentTo(new List<TaskResponse>
        {
            new()
            {
                Id = 1,
                Name = "Task 1",
                Description = "Test description 1",
                Status = Status.NotStarted,
                AssignedTo = "user1@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Task 2",
                Description = "Test description 2",
                Status = Status.InProgress,
                AssignedTo = "user2@example.com"
            },
            new()
            {
                Id = 3,
                Name = "Task 3",
                Description = "Test description 3",
                Status = Status.Completed,
                AssignedTo = "user3@example.com"
            }
        });
    }

    private List<TaskEntity> GetMockedTasks()
    {
        return new List<TaskEntity>
        {
            new()
            {
                Id = 1,
                Name = "Task 1",
                Description = "Test description 1",
                Status = Status.NotStarted,
                AssignedTo = "user1@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Task 2",
                Description = "Test description 2",
                Status = Status.InProgress,
                AssignedTo = "user2@example.com"
            },
            new()
            {
                Id = 3,
                Name = "Task 3",
                Description = "Test description 3",
                Status = Status.Completed,
                AssignedTo = "user3@example.com"
            }
        };
    }
}
