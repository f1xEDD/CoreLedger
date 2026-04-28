using CoreLedger.Application.Abstractions;
using CoreLedger.Application.Transfers;

namespace CoreLedger.Application.Services;

public interface ITransferQueryService
{
    Task<Result<TransferDetailsDto>> GetByIdAsync(Guid transferId, CancellationToken ct);
}
