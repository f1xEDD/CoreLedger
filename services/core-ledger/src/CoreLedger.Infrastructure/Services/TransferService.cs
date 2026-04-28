using System.Data;
using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Services;
using CoreLedger.Application.Transfers.Events;
using CoreLedger.Domain.Ledger;
using CoreLedger.Domain.Time;
using CoreLedger.Domain.Transfers;
using CoreLedger.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

public sealed class TransferService(
    LedgerDbContext db,
    ITimeProvider timeProvider,
    ILogger<TransferService> logger) : ITransferService
{
    public async Task<Result<Guid>> CreateAsync(
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
            logger.LogInformation("Idempotent hit: transfer_id={TransferId} key={Key}", existingId, idk);

            return Result<Guid>.Ok(existingId);
        }
        
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);

        var accounts = await db.Accounts
            .FromSqlInterpolated(
            $"""
                SELECT a.*
                FROM accounts AS a
                WHERE a.account_id IN ({fromId}, {toId})
                FOR UPDATE
            """)
            .AsTracking()
            .ToListAsync(ct);

        var from = accounts.SingleOrDefault(a => a.AccountId == fromId);

        if (from is null)
        {
            return Result<Guid>.Fail(AppError.NotFound("From account not found."));
        }

        var to = accounts.SingleOrDefault(a => a.AccountId == toId);

        if (to is null)
        {
            return Result<Guid>.Fail(AppError.NotFound("To account not found."));
        }

        if (!string.Equals(from.Currency, to.Currency, StringComparison.Ordinal))
        {
            return Result<Guid>.Fail(AppError.Invalid("Accounts must share the same currency"));
        }
        
        var money = new Money(amount, currency ?? from.Currency);

        var transferId = Guid.NewGuid();
        var domain = Transfer.Create(transferId, from, to, money, booking, value);

        db.Transfers.Add(domain);
        db.LedgerEntries.AddRange(domain.Entries);

        db.Entry(domain).Property("IdempotencyKey").CurrentValue = idk;

        var transferCreated = new TransferCreatedEvent(
            TransferId: transferId,
            FromAccountId: from.AccountId,
            ToAccountId: to.AccountId,
            Amount: amount,
            Currency: money.Currency,
            BookingDate: booking,
            ValueDate: value,
            OccurredAtUtc: timeProvider.UtcNow);

        var outboxMessage = OutboxMessageFactory.From(
            type: TransferCreatedEvent.EventType,
            message: transferCreated,
            occurredAtUtc: transferCreated.OccurredAtUtc);

        db.OutboxMessages.Add(outboxMessage);
        
        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Transfer created: transfer_id={TransferId} from={From} to={To} amount={Amount} {Currency}",
                transferId, fromId, toId, amount, currency ?? "RUB");

            return Result<Guid>.Ok(transferId);
        }
        catch (DbUpdateException ex) when (IsUniqueViolationOnIdempotencyKey(ex))
        {
            await transaction.RollbackAsync(ct);

            var id = await db.Transfers
                .Where(t => EF.Property<string>(t, "IdempotencyKey") == idk)
                .Select(t => t.TransferId)
                .FirstAsync(ct);

            logger.LogInformation(
                "Idempotent race resolved: returning existing transfer_id={TransferId} key={Key}",
                id, idk);

            return Result<Guid>.Ok(id);
        }
    }

    private static bool IsUniqueViolationOnIdempotencyKey(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("idempotency_key", StringComparison.OrdinalIgnoreCase) == true;
}