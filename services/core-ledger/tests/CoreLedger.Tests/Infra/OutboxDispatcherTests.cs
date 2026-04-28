using CoreLedger.Application.Outbox;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Options;
using CoreLedger.Infrastructure.Outbox;
using CoreLedger.Tests.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CoreLedger.Tests.Infra;

[Collection("pg")]
public class OutboxDispatcherTests(TestPostgresFixture fixture)
{
    private TestEnv CreateEnv() => new(fixture.CreateDbContext());
    
    [Fact]
    public async Task DispatchOnceAsync_ShouldMarkPendingMessageAsProcessed_WhenPublishSucceeds()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var outboxMessage = new OutboxMessage(
            id: Guid.NewGuid(),
            type: "coreledger.test.v1",
            payload: "{}",
            occurredAtUtc: DateTime.UtcNow);

        env.Db.OutboxMessages.Add(outboxMessage);
        await env.Db.SaveChangesAsync();

        var fakePublisher = new FakeEventPublisher();

        var services = new ServiceCollection();

        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(fixture.ConnectionString));

        services.AddSingleton<IEventPublisher>(fakePublisher);

        var provider = services.BuildServiceProvider();

        var dispatcher = new OutboxDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxOptions
            {
                BatchSize = 10,
                PollingIntervalSeconds = 1,
                MaxAttempts = 5
            }),
            NullLogger<OutboxDispatcher>.Instance);

        await dispatcher.DispatchOnceAsync(CancellationToken.None);

        fakePublisher.Published.Should().HaveCount(1);
        fakePublisher.Published[0].Type.Should().Be("coreledger.test.v1");

        env.Db.ChangeTracker.Clear();
        
        var message = await env.Db.OutboxMessages.SingleAsync();
        message.Status.Should().Be(OutboxMessageStatus.Processed);
        message.ProcessedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task DispatchOnceAsync_ShouldKeepMessagePending_WhenPublishFails_AndRetryIsAllowed()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var outboxMessage = new OutboxMessage(
            id: Guid.NewGuid(),
            type: "coreledger.test.v1",
            payload: "{}",
            occurredAtUtc: DateTime.UtcNow);

        env.Db.OutboxMessages.Add(outboxMessage);
        await env.Db.SaveChangesAsync();

        var services = new ServiceCollection();

        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(fixture.ConnectionString));

        services.AddSingleton<IEventPublisher>(new FailingEventPublisher());

        var provider = services.BuildServiceProvider();

        var dispatcher = new OutboxDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxOptions
            {
                BatchSize = 10,
                PollingIntervalSeconds = 1,
                MaxAttempts = 5
            }),
            NullLogger<OutboxDispatcher>.Instance);

        await dispatcher.DispatchOnceAsync(CancellationToken.None);

        env.Db.ChangeTracker.Clear();

        var message = await env.Db.OutboxMessages.SingleAsync();
        message.Status.Should().Be(OutboxMessageStatus.Pending);
        message.Attempts.Should().Be(1);
        message.LastError.Should().Contain("Publish failed");
    }

    [Fact]
    public async Task DispatchOnceAsync_ShouldMarkMessageAsFailed_WhenPublishFails_AndMaxAttemptsIsOne()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var outboxMessage = new OutboxMessage(
            id: Guid.NewGuid(),
            type: "coreledger.test.v1",
            payload: "{}",
            occurredAtUtc: DateTime.UtcNow);

        env.Db.OutboxMessages.Add(outboxMessage);
        await env.Db.SaveChangesAsync();

        var services = new ServiceCollection();

        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(fixture.ConnectionString));

        services.AddSingleton<IEventPublisher>(new FailingEventPublisher());

        var provider = services.BuildServiceProvider();

        var dispatcher = new OutboxDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxOptions
            {
                BatchSize = 10,
                PollingIntervalSeconds = 1,
                MaxAttempts = 1
            }),
            NullLogger<OutboxDispatcher>.Instance);

        await dispatcher.DispatchOnceAsync(CancellationToken.None);

        env.Db.ChangeTracker.Clear();

        var message = await env.Db.OutboxMessages.SingleAsync();
        message.Status.Should().Be(OutboxMessageStatus.Failed);
        message.Attempts.Should().Be(1);
        message.LastError.Should().Contain("Publish failed");
    }
}
