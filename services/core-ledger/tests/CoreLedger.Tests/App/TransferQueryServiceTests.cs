using CoreLedger.Application.Abstractions;
using CoreLedger.Infrastructure.Services;
using CoreLedger.Tests.Infra;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreLedger.Tests.App;

[Collection("pg")]
public class TransferQueryServiceTests(TestPostgresFixture fixture)
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);
    private static readonly TestTimeProvider TimeProvider = new(DateTime.UtcNow);

    private TestEnv CreateEnv() => new(fixture.CreateDbContext());

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTransferDetails_WhenTransferExists()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var fromId = await env.CreateAccountAsync("RUB");
        var toId = await env.CreateAccountAsync("RUB");

        var transferService = new TransferService(env.Db, TimeProvider, NullLogger<TransferService>.Instance);
        var transferId = (await transferService.CreateAsync(
            Guid.NewGuid().ToString("N"),
            fromId,
            toId,
            125.50m,
            "RUB",
            Today,
            Today,
            CancellationToken.None)).Value;

        var queryService = new TransferQueryService(env.Db);

        var result = await queryService.GetByIdAsync(transferId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TransferId.Should().Be(transferId);
        result.Value.Currency.Should().Be("RUB");
        result.Value.LedgerEntries.Should().HaveCount(2);
        result.Value.LedgerEntries.Select(e => e.AccountId).Should().BeEquivalentTo([fromId, toId]);
        result.Value.LedgerEntries.Select(e => e.Amount).Should().OnlyContain(amount => amount == 125.50m);
        result.Value.LedgerEntries.Select(e => e.Currency).Should().OnlyContain(currency => currency == "RUB");
        result.Value.LedgerEntries.Select(e => e.Direction).Should().BeEquivalentTo(["Credit", "Debit"]);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenTransferMissing()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var queryService = new TransferQueryService(env.Db);

        var result = await queryService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<NotFoundError>();
    }
}
