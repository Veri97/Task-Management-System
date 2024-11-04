using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksManagement.Infrastructure.Persistence;

namespace TasksManagement.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TasksDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("TasksDbConnection"));
        });

        return services;
    }
}
