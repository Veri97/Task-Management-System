using System.Text.Json.Serialization;

namespace TasksManagement.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    NotStarted = 1,
    InProgress,
    Completed
}
