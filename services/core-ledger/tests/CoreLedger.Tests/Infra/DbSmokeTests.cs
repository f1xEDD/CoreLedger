using CoreLedger.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Tests.Infra;

public class DbSmokeTests: IAsyncLifetime
{
    private IContainer? _pg;

    public async Task InitializeAsync()
    {
        _pg = new ContainerBuilder()
            .WithImage("postgres:16-alpine")
            .WithEnvironment("POSTGRES_USER", "dev")
            .WithEnvironment("POSTGRES_PASSWORD", "devpass")
            .WithEnvironment("POSTGRES_DB", "coreledger_test")
            .WithPortBinding(6543, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();
        
        await _pg.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_pg != null)
        {
            await _pg.DisposeAsync();
        }
    }

    [Fact]
    public async Task Should_Create_Database_Schema()
    {
        //Arrange
        const string connectionString = "Host=localhost;Port=6543;Database=coreledger_test;Username=dev;Password=devpass";
        
        var options = new DbContextOptionsBuilder<LedgerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        //Act
        await using var context = new LedgerDbContext(options);
        await context.Database.MigrateAsync();
        
        //Assert
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }
}