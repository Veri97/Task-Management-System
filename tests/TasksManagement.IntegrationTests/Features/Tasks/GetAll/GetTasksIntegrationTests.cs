using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TasksManagement.Application.Features.Tasks.GetAll;
using TasksManagement.Core.Enums;
using TasksManagement.IntegrationTests.Shared;

namespace TasksManagement.IntegrationTests.Features.Tasks.GetAll;

[Collection(nameof(SharedIntegrationTestCollection))]
public class GetTasksIntegrationTests
{
    private readonly HttpClient _httpClient;

    public GetTasksIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _httpClient = factory.HttpClient;
    }

    [Fact]
    public async Task GetTasksApiEndpoint_ShouldReturnListOfTasks_WhenTasksExist()
    {
        // Act
        var responseMessage = await _httpClient.GetAsync("api/tasks");
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasksResponse = await responseMessage.Content.ReadFromJsonAsync<List<TaskResponse>>();

        // Assert
        VerifyTaskListResponse(tasksResponse!);
    }

    private void VerifyTaskListResponse(List<TaskResponse> response)
    {
        response.Should().NotBeEmpty();
        response[0].Should().BeEquivalentTo(new TaskResponse
        {
            Id = 1,
            Name = "Task 123",
            Description = "test description",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        });
        response[1].Should().BeEquivalentTo(new TaskResponse
        {
            Id = 2,
            Name = "Test Test",
            Description = "test description 456",
            Status = Status.InProgress,
            AssignedTo = "user2@example.com"
        });
    }
}
