using System.Data;
using CoreLedger.Application.Services;
using CoreLedger.Domain.Ledger;
using CoreLedger.Domain.Transfers;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services;

public sealed class TransferService(LedgerDbContext db) : ITransferService
{
    public async Task<Guid> CreateAsync(
        string idk,
        Guid fromId,
        Guid toId,
        decimal amount,
        string? currency,
        DateOnly booking,
        DateOnly value,
        CancellationToken ct)
    {
        var existingId = await db.Transfers
            .Where(t => EF.Property<string>(t, "IdempotencyKey") == idk)
            .Select(t => t.TransferId)
            .FirstOrDefaultAsync(ct);

        if (existingId != Guid.Empty)
        {
            return existingId;
        }

        // 2) Транзакция + row-locks
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);

        var accounts = await db.Accounts
            .FromSqlInterpolated($"""
                SELECT a.*
                FROM accounts AS a
                WHERE a.account_id IN ({fromId}, {toId})
                FOR UPDATE
            """)
            .AsTracking()
            .ToListAsync(ct);

        var from = accounts.SingleOrDefault(a => a.AccountId == fromId) ??
                   throw new InvalidOperationException("From account not found.");
        
        var to = accounts.SingleOrDefault(a => a.AccountId == toId) ??
                 throw new InvalidOperationException("To account not found.");

        var money = new Money(amount, currency ?? from.Currency);

        var transferId = Guid.NewGuid();
        var domain = Transfer.Create(transferId, from, to, money, booking, value);

        db.Transfers.Add(domain);
        db.LedgerEntries.AddRange(domain.Entries);

        db.Entry(domain).Property("IdempotencyKey").CurrentValue = idk;

        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return transferId;
        }
        catch (DbUpdateException ex) when (IsUniqueViolationOnIdempotencyKey(ex))
        {
            await transaction.RollbackAsync(ct);

            var id = await db.Transfers
                .Where(t => EF.Property<string>(t, "IdempotencyKey") == idk)
                .Select(t => t.TransferId)
                .FirstAsync(ct);

            return id;
        }
    }

    private static bool IsUniqueViolationOnIdempotencyKey(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("idempotency_key", StringComparison.OrdinalIgnoreCase) == true;
}