using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Services;
using CoreLedger.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Services;

public sealed class AccountService(LedgerDbContext db) : IAccountService
{
    public async Task<Result<Guid>> CreateAsync(string currency, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result<Guid>.Fail(AppError.Invalid("Currency must be provided"));
        }

        var account = new Account(Guid.NewGuid(), Guid.NewGuid(), currency);
        
        db.Accounts.Add(account);
        
        await db.SaveChangesAsync(ct);
        
        return Result<Guid>.Ok(account.AccountId);
    }

    public async Task<Result<bool>> CloseAsync(Guid accountId, CancellationToken ct)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId, ct);
        
        if (account is null)
        {
            return Result<bool>.Fail(AppError.NotFound("Account not found"));
        }

        if (account.Status == AccountStatus.Closed)
        {
            return Result<bool>.Fail(AppError.Invalid("Account already closed"));
        }
        
        typeof(Account).GetProperty(nameof(Account.Status))!.SetValue(account, AccountStatus.Closed);
        
        await db.SaveChangesAsync(ct);
        
        return Result<bool>.Ok(true);
    }
}