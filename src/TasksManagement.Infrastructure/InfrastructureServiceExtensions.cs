using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using TasksManagement.Application.Abstractions;
using TasksManagement.Core.Contracts;
using TasksManagement.Infrastructure.Persistence;
using TasksManagement.Infrastructure.Persistence.Repositories;
using TasksManagement.Infrastructure.MessageBroker;
using TasksManagement.Application.Settings;

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

        services.AddSingleton<IRabbitMQClient, RabbitMQClient>();

        var rabbitMQSettings = config.GetSection(RabbitMQSettings.Name)
                                      .Get<RabbitMQSettings>();

        services
            .AddSingleton<IConnectionFactory>(serviceProvider =>
            {
                var uri = new Uri(rabbitMQSettings!.ConnectionString);
                return new ConnectionFactory
                {
                    Uri = uri,
                    DispatchConsumersAsync = true,
                };
            });

        return services;
    }
}
