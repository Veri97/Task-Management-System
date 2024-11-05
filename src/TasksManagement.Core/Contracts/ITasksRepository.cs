using TasksManagement.Core.Entities;

namespace TasksManagement.Core.Contracts;

public interface ITasksRepository
{
    Task Create(TaskEntity task, CancellationToken cancellationToken);
    Task<bool> TaskByNameExists(string taskName, CancellationToken cancellationToken);
    Task<TaskEntity?> GetTaskById(int id, CancellationToken cancellationToken);
    Task<List<TaskEntity>> GetAllTasks(CancellationToken cancellationToken);
}
