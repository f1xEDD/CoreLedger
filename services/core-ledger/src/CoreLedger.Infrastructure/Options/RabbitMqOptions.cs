namespace CoreLedger.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "dev";

    public string Password { get; init; } = "devpass";

    public string ExchangeName { get; init; } = "coreledger.events";

    public string ExchangeType { get; init; } = "topic";
}