using Microsoft.EntityFrameworkCore;
using TasksManagement.Infrastructure.Persistence;

namespace TasksManagement.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using TasksDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogCritical(ex, "An error ocurred during migration!");
        }
    }
}
