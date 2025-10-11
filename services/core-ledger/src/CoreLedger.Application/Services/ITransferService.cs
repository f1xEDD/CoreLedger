using CoreLedger.Application.Abstractions;

namespace CoreLedger.Application.Services;

public interface ITransferService
{
    Task<Result<Guid>> CreateAsync(
        string idempotencyKey,
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string? currency,
        DateOnly bookingDate,
        DateOnly valueDate,
        CancellationToken ct);
}