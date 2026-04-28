using CoreLedger.Domain.Accounts;
using CoreLedger.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Tests.Infra;

public sealed class TestEnv(LedgerDbContext db) : IAsyncDisposable
{
    public LedgerDbContext Db { get; } = db;

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
    }

    public async Task<Guid> CreateAccountAsync(string currency = "RUB", AccountStatus status = AccountStatus.Active)
    {
        var acc = new Account(Guid.NewGuid(), Guid.NewGuid(), currency, status);
        Db.Accounts.Add(acc);
        await Db.SaveChangesAsync();
        return acc.AccountId;
    }

    public async Task TruncateAllAsync()
    {
        await Db.Database.ExecuteSqlRawAsync("DELETE FROM public.outbox_messages;");
        await Db.Database.ExecuteSqlRawAsync("DELETE FROM public.ledger_entries;");
        await Db.Database.ExecuteSqlRawAsync("DELETE FROM public.transfers;");
        await Db.Database.ExecuteSqlRawAsync("DELETE FROM public.accounts;");
    }
}
