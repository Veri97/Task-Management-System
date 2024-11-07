using Microsoft.EntityFrameworkCore;
using TasksManagement.Core.Contracts;
using TasksManagement.Core.Entities;

namespace TasksManagement.Infrastructure.Persistence.Repositories;

public class TasksRepository : ITasksRepository
{
    private readonly TasksDbContext _dbContext;

    public TasksRepository(TasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Create(TaskEntity task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
    }

    public async Task<List<TaskEntity>> GetAllTasks(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity?> GetTaskById(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> Exists(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> Exists(string taskName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.AnyAsync(x => x.Name == taskName, cancellationToken);
    }  
}
