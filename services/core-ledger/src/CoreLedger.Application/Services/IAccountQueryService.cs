using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Accounts;

namespace CoreLedger.Application.Services;

public interface IAccountQueryService
{
    Task<Result<AccountBalanceDto>> GetBalanceAsync(Guid accountId, CancellationToken ct);
}
