using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Services;
using CoreLedger.Application.Transfers;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services;

public sealed class TransferQueryService(LedgerDbContext db) : ITransferQueryService
{
    public async Task<Result<TransferDetailsDto>> GetByIdAsync(Guid transferId, CancellationToken ct)
    {
        var transfer = await db.Transfers
            .AsNoTracking()
            .Where(t => t.TransferId == transferId)
            .Select(t => new
            {
                t.TransferId,
                t.Currency
            })
            .FirstOrDefaultAsync(ct);

        if (transfer is null)
        {
            return Result<TransferDetailsDto>.Fail(AppError.NotFound("Transfer not found."));
        }

        var ledgerEntries = await db.LedgerEntries
            .AsNoTracking()
            .Where(e => e.TransferId == transferId)
            .OrderBy(e => e.Direction)
            .ThenBy(e => e.EntryId)
            .Select(e => new LedgerEntryDto(
                e.EntryId,
                e.AccountId,
                e.Amount.Amount,
                e.Amount.Currency,
                e.Direction.ToString(),
                e.BookingDate,
                e.ValueDate))
            .ToListAsync(ct);

        return Result<TransferDetailsDto>.Ok(new TransferDetailsDto(
            transfer.TransferId,
            transfer.Currency,
            ledgerEntries));
    }
}
