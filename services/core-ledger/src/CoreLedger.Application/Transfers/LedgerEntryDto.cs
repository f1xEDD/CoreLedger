namespace CoreLedger.Application.Transfers;

public sealed record LedgerEntryDto(
    Guid EntryId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction,
    DateOnly BookingDate,
    DateOnly ValueDate);
