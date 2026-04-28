using CoreLedger.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Api;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();

        try
        {
            logger.LogInformation("Applying database migrations...");

            await db.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations.");
            throw;
        }
    }
}