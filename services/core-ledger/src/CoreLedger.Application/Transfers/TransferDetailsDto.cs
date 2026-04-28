namespace CoreLedger.Application.Transfers;

public sealed record TransferDetailsDto(
    Guid TransferId,
    string Currency,
    IReadOnlyList<LedgerEntryDto> LedgerEntries);
