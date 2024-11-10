using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Services;
using TasksManagement.Application.Settings;
using TasksManagement.Core.Enums;
using FluentAssertions;

namespace TasksManagement.UnitTests.Services;

public class ServiceBusHandlerTests
{
    private readonly Mock<IRabbitMQClient> _rabbitMQClientMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ServiceBusHandler>> _loggerMock;
    private readonly Mock<IOptions<RabbitMQSettings>> _rabbitMQSettingsMock;
    private readonly ServiceBusHandler _serviceBusHandler;

    public ServiceBusHandlerTests()
    {
        _rabbitMQClientMock = new Mock<IRabbitMQClient>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ServiceBusHandler>>();
        _rabbitMQSettingsMock = new Mock<IOptions<RabbitMQSettings>>();
        _rabbitMQSettingsMock.SetupGet(x => x.Value).Returns(new RabbitMQSettings
        {
            ExchangeName = "test_exchange",
            TaskCreateQueueName = "task_create_test_queue",
            RetryCount = 2,
            RetryDurationInSeconds = 0
        });

        _serviceBusHandler = new ServiceBusHandler(
            _serviceProviderMock.Object,
            _rabbitMQClientMock.Object,
            _loggerMock.Object,
            _rabbitMQSettingsMock.Object);
    }

    [Fact]
    public async Task SendMessage_ShouldPublishMessageToQueue_WhenMessageIsSentSuccessfully()
    {
        // Arrange
        var message = new CreateTaskMessage
        {
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        var messageBodyArray = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Act
        await _serviceBusHandler.SendMessage(
            _rabbitMQSettingsMock.Object.Value.TaskCreateQueueName, message);

        // Assert
        _rabbitMQClientMock.Verify(x =>
             x.Publish(
                _rabbitMQSettingsMock.Object.Value.ExchangeName,
                _rabbitMQSettingsMock.Object.Value.TaskCreateQueueName,
                null,
                messageBodyArray),
          Times.Once);

        VerifyLogMessage(
            LogLevel.Information,
            $"Sent '{typeof(CreateTaskMessage).Name}' to '{_rabbitMQSettingsMock.Object.Value.TaskCreateQueueName}' queue",
            Times.Once());
    }

    [Fact]
    public async Task SendMessage_ShouldRetryMessagePublishingAndLogErrors_WhenThereAreErrorsDuringPublishing()
    {
        // Arrange
        var message = new CreateTaskMessage
        {
            Name = "Task 1",
            Description = "Test description 1",
            Status = Status.NotStarted,
            AssignedTo = "user1@example.com"
        };

        var messageBodyArray = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        _rabbitMQClientMock.Setup(x =>
             x.Publish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties?>(), It.IsAny<byte[]>()))
            .Throws(new Exception("Test Error"));

        // Act
        Func<Task> result = async () => await _serviceBusHandler.SendMessage(
            _rabbitMQSettingsMock.Object.Value.TaskCreateQueueName, message);

        // Assert
        await result.Should().ThrowAsync<Exception>().WithMessage("Test Error");

        _rabbitMQClientMock.Verify(x =>
             x.Publish(
                _rabbitMQSettingsMock.Object.Value.ExchangeName,
                _rabbitMQSettingsMock.Object.Value.TaskCreateQueueName,
                null,
                messageBodyArray),
          Times.Exactly(_rabbitMQSettingsMock.Object.Value.RetryCount + 1));

        for (var i = 0; i < _rabbitMQSettingsMock.Object.Value.RetryCount; i++)
        {
            VerifyLogMessage(
            LogLevel.Error,
            $"An exception occurred during message publishing: Test Error. Retrying attempt {i + 1}...",
            Times.Once());
        }
    }

    private void VerifyLogMessage(LogLevel logLevel, string message, Times numberOfMethodInvocations)
    {
        _loggerMock.Verify(l =>
           l.Log(logLevel,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, _) =>
                   v.ToString().Contains(message)),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception, string>>()
           ), numberOfMethodInvocations);
    }
}
