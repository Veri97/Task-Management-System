using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TasksManagement.Api.Errors;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Core.Enums;
using TasksManagement.Infrastructure.Persistence;
using TasksManagement.IntegrationTests.Shared;

namespace TasksManagement.IntegrationTests.Features.Tasks.Update;

[Collection(nameof(SharedIntegrationTestCollection))]
public class UpdateTaskIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TasksDbContext _context;
    private readonly IServiceScope _scope;

    public UpdateTaskIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _httpClient = factory.HttpClient;
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetService<TasksDbContext>()!;
    }

    [Fact]
    public async Task UpdateTaskApiEndpoint_ShouldReturnBadRequestResponse_WhenApiRequestHasValidationErrors()
    {
        // Arrange
        var request = new UpdateTaskRequest
        {
            NewStatus = (Status)5
        };

        // Act
        var responseMessage = await _httpClient.PutAsJsonAsync("api/tasks/-2", request);
        var apiErrorResponse = await responseMessage.Content.ReadFromJsonAsync<ApiErrorResponse>();

        // Assert
        apiErrorResponse.Should().NotBeNull();
        apiErrorResponse!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        apiErrorResponse!.Errors.Should().BeEquivalentTo(new List<string>
        {
           "Task Id must be greater than 0",
           "Task status is not valid"
        });
    }

    [Fact]
    public async Task UpdateTaskApiEndpoint_ShouldReturnNotFoundResponse_WhenTaskDoesNotExist()
    {
        // Arrange
        const int taskId = 123;

        var request = new UpdateTaskRequest
        {
            NewStatus = Status.Completed
        };

        // Act
        var responseMessage = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}", request);
        var apiErrorResponse = await responseMessage.Content.ReadFromJsonAsync<ApiErrorResponse>();

        // Assert
        apiErrorResponse.Should().NotBeNull();
        apiErrorResponse!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        apiErrorResponse!.Errors.Should().BeEquivalentTo(new List<string>
        {
           $"Task with id {taskId} does not exist"
        });
    }

    [Fact]
    public async Task UpdateTaskApiEndpoint_ShouldSuccessfullyUpdateTaskStatus_WhenTaskExists()
    {
        // Arrange
        const int taskId = 3;

        var request = new UpdateTaskRequest
        {
            NewStatus = Status.Completed
        };

        // Act
        var responseMessage = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}", request);
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert
        await Task.Delay(500);

        var updatedTask = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);
        updatedTask!.Status.Should().Be(request.NewStatus);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _context?.Dispose();
    }
}
