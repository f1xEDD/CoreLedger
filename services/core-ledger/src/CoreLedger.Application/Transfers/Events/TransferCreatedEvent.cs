namespace CoreLedger.Application.Transfers.Events;

public sealed record TransferCreatedEvent(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    DateOnly BookingDate,
    DateOnly ValueDate,
    DateTime OccurredAtUtc)
{
    public const string EventType = "coreledger.transfer.created.v1";
}