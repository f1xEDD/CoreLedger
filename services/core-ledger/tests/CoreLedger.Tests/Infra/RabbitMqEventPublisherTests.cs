using System.Text;
using CoreLedger.Infrastructure.Options;
using CoreLedger.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CoreLedger.Tests.Infra;

[Collection("rabbitmq")]
public sealed class RabbitMqEventPublisherTests
{
    private readonly TestRabbitMqFixture _fixture;

    public RabbitMqEventPublisherTests(TestRabbitMqFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishMessageToBoundQueue()
    {
        const string exchangeName = "coreledger.events.test";
        const string eventType = "coreledger.transfer.created.v1";
        const string queueName = "coreledger.publisher.test";

        const string payload = """
                               {
                                 "transferId": "11111111-1111-1111-1111-111111111111",
                                 "amount": 100,
                                 "currency": "RUB"
                               }
                               """;

        var options = Options.Create(new RabbitMqOptions
        {
            HostName = _fixture.HostName,
            Port = _fixture.AmqpPort,
            UserName = _fixture.UserName,
            Password = _fixture.Password,
            ExchangeName = exchangeName,
            ExchangeType = ExchangeType.Topic
        });

        var publisher = new RabbitMqEventPublisher(
            options,
            NullLogger<RabbitMqEventPublisher>.Instance);

        var factory = new ConnectionFactory
        {
            HostName = _fixture.HostName,
            Port = _fixture.AmqpPort,
            UserName = _fixture.UserName,
            Password = _fixture.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: eventType);
        
        while (true)
        {
            var oldMessage = await channel.BasicGetAsync(queueName, autoAck: true);
            if (oldMessage is null)
                break;
        }
        
        await publisher.PublishAsync(
            type: eventType,
            payload: payload,
            cancellationToken: CancellationToken.None);
        
        var result = await WaitForMessageAsync(channel, queueName);

        result.Should().NotBeNull();

        var body = Encoding.UTF8.GetString(result!.Body.ToArray());

        body.Should().Be(payload);

        result.Exchange.Should().Be(exchangeName);
        result.RoutingKey.Should().Be(eventType);
        result.BasicProperties.ContentType.Should().Be("application/json");
        result.BasicProperties.Type.Should().Be(eventType);
        result.BasicProperties.Persistent.Should().BeTrue();
    }

    private static async Task<BasicGetResult?> WaitForMessageAsync(
        IChannel channel,
        string queueName)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            var result = await channel.BasicGetAsync(queueName, autoAck: true);

            if (result is not null)
                return result;

            await Task.Delay(100);
        }

        return null;
    }
}