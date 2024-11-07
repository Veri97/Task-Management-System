namespace TasksManagement.Application.Settings;

public class RabbitMQSettings
{
    public const string Name = nameof(RabbitMQSettings);
    public string ConnectionString { get; set; } = null!;
    public string ExchangeName { get; set; } = null!;
    public string ExchangeType { get; set; } = null!;
    public string TaskCreateQueueName { get; set; } = null!;
    public string TaskUpdateQueueName { get; set; } = null!;
    public string ActionCompletedQueueName { get; set; } = null!;
    public int RetryCount { get; set; }
    public int RetryDurationInSeconds { get; set; }
}
