using CoreLedger.Application.Abstractions;

namespace CoreLedger.Application.Services;

public interface IAccountService
{
    Task<Result<Guid>> CreateAsync(string currency, CancellationToken ct);
    Task<Result<bool>> CloseAsync(Guid accountId, CancellationToken ct);
}