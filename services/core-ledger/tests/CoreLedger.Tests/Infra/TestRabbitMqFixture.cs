using Testcontainers.RabbitMq;

namespace CoreLedger.Tests.Infra;

public sealed class TestRabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer =
        new RabbitMqBuilder("rabbitmq:3-management-alpine")
            .WithUsername("dev")
            .WithPassword("devpass")
            .Build();

    public string HostName => _rabbitMqContainer.Hostname;

    public int AmqpPort => _rabbitMqContainer.GetMappedPublicPort(5672);

    public string UserName => "dev";

    public string Password => "devpass";

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }
}

[CollectionDefinition("rabbitmq")]
public sealed class RabbitMqCollection : ICollectionFixture<TestRabbitMqFixture>
{
}