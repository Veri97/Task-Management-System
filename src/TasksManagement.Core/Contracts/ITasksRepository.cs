using TasksManagement.Core.Entities;

namespace TasksManagement.Core.Contracts;

public interface ITasksRepository
{
    Task Create(TaskEntity task, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, CancellationToken cancellationToken = default);
    Task<bool> Exists(string taskName, CancellationToken cancellationToken = default);
    Task<TaskEntity?> GetTaskById(int id, CancellationToken cancellationToken = default);
    Task<List<TaskEntity>> GetAllTasks(CancellationToken cancellationToken = default);
}
