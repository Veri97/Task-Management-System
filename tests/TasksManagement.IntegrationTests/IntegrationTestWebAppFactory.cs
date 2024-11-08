using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TasksManagement.Infrastructure.Persistence;
using TasksManagement.Api;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using RabbitMQ.Client;

namespace TasksManagement.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<IApiAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(1433)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithPassword("TestPassword1234!")
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

    private readonly RabbitMqContainer _rabbitMQContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:management")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

    public HttpClient HttpClient { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TasksDbContext>));
            services.RemoveAll(typeof(IConnectionFactory));

            services.AddDbContext<TasksDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services
                .AddSingleton<IConnectionFactory>(serviceProvider =>
                {
                    var uri = new Uri(_rabbitMQContainer.GetConnectionString());
                    return new ConnectionFactory
                    {
                        Uri = uri,
                        DispatchConsumersAsync = true
                    };
                });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMQContainer.StartAsync();

        HttpClient = CreateClient();
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetService<TasksDbContext>();

        await context!.Database.EnsureCreatedAsync();
        await CleanDatabase(context!);
        TestDatabase.Seed(context);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _rabbitMQContainer.DisposeAsync();
    }

    private async Task CleanDatabase(TasksDbContext context)
    {
        await context.Tasks.ExecuteDeleteAsync();
    }
}
