using System.Diagnostics;
using CoreLedger.Domain.Abstractions;

namespace CoreLedger.Domain.Ledger;

[DebuggerDisplay("{Direction} {Amount} -> Acc:{AccountId}, Tr:{TransferId}, Val:{ValueDate}")]
public sealed class LedgerEntry
{
    public Guid EntryId { get; }
    public Guid AccountId { get; }
    public Guid TransferId { get; }
    public Money Amount { get; }
    public DateOnly BookingDate { get; }
    public DateOnly ValueDate { get; }
    public EntryDirection Direction { get; }

    public LedgerEntry(
        Guid entryId,
        Guid accountId,
        Guid transferId,
        Money amount,
        DateOnly bookingDate,
        DateOnly valueDate,
        EntryDirection direction)
    {
        if (valueDate < bookingDate)
        {
            Guard.That(valueDate >= bookingDate, "ValueDate must be >= BookingDate.");
        }

        EntryId = entryId;
        AccountId = accountId;
        TransferId = transferId;
        Amount = amount;
        BookingDate = bookingDate;
        ValueDate = valueDate;
        Direction = direction;
    }

    public Money SignedAmount() =>
        Direction == EntryDirection.Debit ? Amount : new Money(-Amount.Amount, Amount.Currency);
}