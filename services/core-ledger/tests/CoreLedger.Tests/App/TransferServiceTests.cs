using CoreLedger.Infrastructure.Services;
using CoreLedger.Tests.Infra;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreLedger.Tests.App;

[Collection("pg")]
public class TransferServiceTests(TestPostgresFixture fixture)
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    private TestEnv CreateEnv() => new(fixture.CreateDbContext());
    
    [Fact]
    public async Task CreateAsync_ShouldCreateTwoBalancedEntries_WhenHappyPath()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var fromId = await env.CreateAccountAsync("RUB");
        var toId   = await env.CreateAccountAsync("RUB");

        var transferService = new TransferService(env.Db, NullLogger<TransferService>.Instance);

        var key = Guid.NewGuid().ToString("N");
        var booking = Today;

        var transferId = await transferService.CreateAsync(
            key,
            fromId,
            toId,
            amount: 100m,
            currency: "RUB",
            booking,
            booking,
            ct: CancellationToken.None);

        var transfers = await env.Db.Transfers.CountAsync();
        transfers.Should().Be(1);

        var entries = await env.Db.LedgerEntries
            .Where(e => e.TransferId == transferId)
            .ToListAsync();

        entries.Should().HaveCount(2);

        var signedSum = entries.Select(e => e.SignedAmount().Amount).Sum();
        signedSum.Should().Be(0m);
    }

    [Fact]
    public async Task CreateAsync_ShouldBeIdempotent_ForSameIdempotencyKey()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var fromId = await env.CreateAccountAsync("RUB");
        var toId   = await env.CreateAccountAsync("RUB");

        var svc = new TransferService(env.Db, NullLogger<TransferService>.Instance);

        var key = Guid.NewGuid().ToString("N");
        var booking = Today;

        var id1 = await svc.CreateAsync(key, fromId, toId, 50m, "RUB", booking, booking, CancellationToken.None);
        var id2 = await svc.CreateAsync(key, fromId, toId, 50m, "RUB", booking, booking, CancellationToken.None);

        id2.Should().Be(id1);

        (await env.Db.Transfers.CountAsync()).Should().Be(1);

        var entries = await env.Db.LedgerEntries.Where(e => e.TransferId == id1).ToListAsync();
        entries.Should().HaveCount(2);
        entries.Sum(e => e.SignedAmount().Amount).Should().Be(0m);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldResultInSingleTransfer_UnderParallelRequests_WithSameKey()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var fromId = await env.CreateAccountAsync("RUB");
        var toId   = await env.CreateAccountAsync("RUB");

        var key = Guid.NewGuid().ToString("N");
        var booking = Today;

        const int parallel = 20;
        
        var tasks = Enumerable.Range(0, parallel).Select(async _ =>
        {
            await using var scopedEnv = CreateEnv();
            var svc = new TransferService(scopedEnv.Db, NullLogger<TransferService>.Instance);
            return await svc.CreateAsync(key, fromId, toId, 10m, "RUB", booking, booking, CancellationToken.None);
        });

        var ids = await Task.WhenAll(tasks);

        ids.Distinct().Should().HaveCount(1);

        (await env.Db.Transfers.CountAsync()).Should().Be(1);

        var transferId = ids.First();
        var entries = await env.Db.LedgerEntries.Where(e => e.TransferId == transferId).ToListAsync();
        entries.Should().HaveCount(2);
        entries.Sum(e => e.SignedAmount().Amount).Should().Be(0m);
    }
}