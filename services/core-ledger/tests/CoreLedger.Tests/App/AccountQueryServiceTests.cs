using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Accounts;
using CoreLedger.Infrastructure.Services;
using CoreLedger.Tests.Infra;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreLedger.Tests.App;

[Collection("pg")]
public class AccountQueryServiceTests(TestPostgresFixture fixture)
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    private TestEnv CreateEnv() => new(fixture.CreateDbContext());

    [Fact]
    public async Task GetBalanceAsync_ShouldCalculateSignedBalanceFromLedgerEntries()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var fromId = await env.CreateAccountAsync("RUB");
        var toId = await env.CreateAccountAsync("RUB");

        var transferService = new TransferService(env.Db, NullLogger<TransferService>.Instance);

        await transferService.CreateAsync(
            Guid.NewGuid().ToString("N"),
            fromId,
            toId,
            125.50m,
            "RUB",
            Today,
            Today,
            CancellationToken.None);

        var queryService = new AccountQueryService(env.Db);

        var fromBalance = await queryService.GetBalanceAsync(fromId, CancellationToken.None);
        var toBalance = await queryService.GetBalanceAsync(toId, CancellationToken.None);

        fromBalance.IsSuccess.Should().BeTrue();
        fromBalance.Value.Should().Be(new AccountBalanceDto(fromId, -125.50m, "RUB"));

        toBalance.IsSuccess.Should().BeTrue();
        toBalance.Value.Should().Be(new AccountBalanceDto(toId, 125.50m, "RUB"));
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnZero_WhenAccountHasNoEntries()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var accountId = await env.CreateAccountAsync("RUB");

        var queryService = new AccountQueryService(env.Db);

        var result = await queryService.GetBalanceAsync(accountId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new AccountBalanceDto(accountId, 0m, "RUB"));
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnNotFound_WhenAccountMissing()
    {
        await using var env = CreateEnv();
        await env.TruncateAllAsync();

        var queryService = new AccountQueryService(env.Db);

        var result = await queryService.GetBalanceAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<NotFoundError>();
    }
}
