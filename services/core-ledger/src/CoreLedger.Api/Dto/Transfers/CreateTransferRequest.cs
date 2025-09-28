namespace CoreLedger.Api.Dto.Transfers;

public sealed record CreateTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Currency,
    DateOnly? BookingDate,
    DateOnly? ValueDate);