using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksManagement.Core.Contracts;
using TasksManagement.Infrastructure.Persistence;
using TasksManagement.Infrastructure.Persistence.Repositories;

namespace TasksManagement.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITasksRepository, TasksRepository>();

        services.AddDbContext<TasksDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("TasksDbConnection"));
        });

        return services;
    }
}
