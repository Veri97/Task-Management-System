using TasksManagement.Core.Entities;
using TasksManagement.Core.Enums;
using TasksManagement.Infrastructure.Persistence;

namespace TasksManagement.IntegrationTests;

public static class TestDatabase
{
    public static void Seed(TasksDbContext context)
    {
        var tasks = new List<TaskEntity>
        {
            new TaskEntity
            {
                Name = "Task 123",
                Description = "test description",
                Status = Status.NotStarted,
                AssignedTo = "user1@example.com"
            },
            new TaskEntity
            {
                Name = "Test Test",
                Description = "test description 456",
                Status = Status.InProgress,
                AssignedTo = "user2@example.com"
            },
            new TaskEntity
            {
                Name = "Test Test 123",
                Description = "test description 123",
                Status = Status.NotStarted,
                AssignedTo = "user3@example.com"
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}
