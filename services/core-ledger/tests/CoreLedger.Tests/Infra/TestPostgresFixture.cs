using CoreLedger.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Tests.Infra;

public sealed class TestPostgresFixture : IAsyncLifetime
{
    private IContainer? _pg;
    public string ConnectionString { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        const int hostPort = 6544;

        _pg = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithEnvironment("POSTGRES_USER", "dev")
            .WithEnvironment("POSTGRES_PASSWORD", "devpass")
            .WithEnvironment("POSTGRES_DB", "coreledger_test")
            .WithPortBinding(hostPort, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();

        await _pg.StartAsync();

        ConnectionString = $"Host=localhost;Port={hostPort};Database=coreledger_test;Username=dev;Password=devpass";

        // Прогоняем миграции один раз для контейнера
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var ctx = new LedgerDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_pg is not null)
        {
            await _pg.DisposeAsync();
        }
    }

    public LedgerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        
        return new LedgerDbContext(options);
    }
}