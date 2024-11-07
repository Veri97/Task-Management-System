using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksManagement.Application.Abstractions;
using TasksManagement.Application.Behaviors;
using TasksManagement.Application.Events;
using TasksManagement.Application.Features.Tasks.Create;
using TasksManagement.Application.Features.Tasks.Update;
using TasksManagement.Application.Services;
using TasksManagement.Application.Settings;

namespace TasksManagement.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        services.AddSingleton<IServiceBusHandler, ServiceBusHandler>();

        services.AddScoped<IMessageConsumer<CreateTaskMessage>, CreateTaskMessageConsumer>();
        services.AddScoped<IMessageConsumer<UpdateTaskMessage>, UpdateTaskMessageConsumer>();
        services.AddScoped<IMessageConsumer<ActionCompletedEvent>, ActionCompletedEventHandler>();

        services.Configure<RabbitMQSettings>(
                options => config.GetSection(RabbitMQSettings.Name).Bind(options));

        return services;
    }
}
