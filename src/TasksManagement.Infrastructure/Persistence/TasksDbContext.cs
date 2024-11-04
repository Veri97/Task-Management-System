using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TasksManagement.Core.Entities;

namespace TasksManagement.Infrastructure.Persistence;

public sealed class TasksDbContext(DbContextOptions<TasksDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskEntity> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
