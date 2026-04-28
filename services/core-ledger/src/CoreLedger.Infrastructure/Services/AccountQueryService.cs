using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Accounts;
using CoreLedger.Application.Services;
using CoreLedger.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services;

public sealed class AccountQueryService(LedgerDbContext db) : IAccountQueryService
{
    public async Task<Result<AccountBalanceDto>> GetBalanceAsync(Guid accountId, CancellationToken ct)
    {
        var account = await db.Accounts
            .AsNoTracking()
            .Where(a => a.AccountId == accountId)
            .Select(a => new
            {
                a.AccountId,
                a.Currency
            })
            .FirstOrDefaultAsync(ct);

        if (account is null)
        {
            return Result<AccountBalanceDto>.Fail(AppError.NotFound("Account not found"));
        }

        var balance = await db.LedgerEntries
            .AsNoTracking()
            .Where(e => e.AccountId == accountId)
            .Select(e => (decimal?)(e.Direction == EntryDirection.Debit
                ? e.Amount.Amount
                : -e.Amount.Amount))
            .SumAsync(ct) ?? 0m;

        return Result<AccountBalanceDto>.Ok(new AccountBalanceDto(
            account.AccountId,
            balance,
            account.Currency));
    }
}
