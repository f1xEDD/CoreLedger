using CoreLedger.Domain.Accounts;
using CoreLedger.Domain.Ledger;
using CoreLedger.Domain.Transfers;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure;

public sealed class LedgerDbContext(DbContextOptions<LedgerDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);
    }
}