using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TasksManagement.Api.Errors;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Core.Enums;
using TasksManagement.Infrastructure.Persistence;
using TasksManagement.IntegrationTests.Shared;

namespace TasksManagement.IntegrationTests.Features.Tasks.Create;

[Collection(nameof(SharedIntegrationTestCollection))]
public class CreateTaskIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TasksDbContext _context;
    private readonly IServiceScope _scope;

    public CreateTaskIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _httpClient = factory.HttpClient;
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetService<TasksDbContext>()!;
    }

    [Fact]
    public async Task CreateTaskApiEndpoint_ShouldReturnBadRequestResponse_WhenApiRequestHasValidationErrors()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Name = "",
            Description = "",
            Status = (Status?)5,
            AssignedTo = "user1@example.com"
        };

        // Act
        var responseMessage = await _httpClient.PostAsJsonAsync("api/tasks", command);
        var apiErrorResponse = await responseMessage.Content.ReadFromJsonAsync<ApiErrorResponse>();

        // Assert
        apiErrorResponse.Should().NotBeNull();
        apiErrorResponse!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        apiErrorResponse!.Errors.Should().BeEquivalentTo(new List<string>
        {
           "Task name is required",
           "Task description is required",
           "Task status is not valid"
        });
    }

    [Fact]
    public async Task CreateTaskApiEndpoint_ShouldReturnConflictResponse_WhenTaskIsNotUnique()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Name = "Task 123",
            Description = "task 123 description",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        // Act
        var responseMessage = await _httpClient.PostAsJsonAsync("api/tasks", command);
        var apiErrorResponse = await responseMessage.Content.ReadFromJsonAsync<ApiErrorResponse>();

        // Assert
        apiErrorResponse.Should().NotBeNull();
        apiErrorResponse!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        apiErrorResponse!.Errors.Should().BeEquivalentTo(new List<string>
        {
            "A task with the same name already exists"
        });
    }

    [Fact]
    public async Task CreateTaskApiEndpoint_ShouldReturnOkResponseAndCreateTask_WhenInputIsValidAndTaskIsUniqueByName()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Name = "task 1",
            Description = "task 1 description",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        // Act
        var responseMessage = await _httpClient.PostAsJsonAsync("api/tasks", command);
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        await Task.Delay(500);
        await AssertTaskIsCreated(command);
    }

    private async Task AssertTaskIsCreated(CreateTaskCommand command)
    {
        var createdTask = await _context.Tasks.FirstOrDefaultAsync(x => x.Name == command.Name);
        createdTask.Should().NotBeNull();
        createdTask!.Name.Should().Be(command.Name);
        createdTask.Description.Should().Be(command.Description);
        createdTask.Status.Should().Be(command.Status);
        createdTask.AssignedTo.Should().Be(command.AssignedTo);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _context?.Dispose();
    }
}
