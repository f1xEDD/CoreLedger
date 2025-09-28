using System.Diagnostics;
using CoreLedger.Domain.Abstractions;
using CoreLedger.Domain.Accounts;
using CoreLedger.Domain.Ledger;

namespace CoreLedger.Domain.Transfers;

[DebuggerDisplay("Tr:{TransferId} {Currency} entries:{Entries.Count}")]
public sealed class Transfer
{
    public Guid TransferId { get; }
    public string Currency { get; }
    public IReadOnlyList<LedgerEntry> Entries => _entries;
    private readonly List<LedgerEntry> _entries = [];

    private Transfer(Guid id, string currency)
    {
        TransferId = id;
        Currency = currency;
    }

    public static Transfer Create(Guid transferId, Account from, Account to, Money amount, DateOnly bookingDate, DateOnly valueDate)
    {
        Guard.AgainstDefault(transferId, nameof(transferId));
        
        Guard.Require(from.AccountId != to.AccountId, "From and To must be different accounts.");
        
        Guard.Require(from.IsActive && to.IsActive, "Both accounts must be active.");
        
        Guard.Require(
            string.Equals(from.Currency, to.Currency, StringComparison.Ordinal),
            "Accounts must share the same currency.");
        
        amount.EnsureSameCurrency(new Money(0m, from.Currency));

        Guard.That(valueDate >= bookingDate, "ValueDate must be >= BookingDate.");
        
        var transfer = new Transfer(transferId, from.Currency);
        
        transfer._entries.Add(new LedgerEntry(Guid.NewGuid(), from.AccountId, transferId, amount, bookingDate, valueDate, EntryDirection.Credit));
        transfer._entries.Add(new LedgerEntry(Guid.NewGuid(), to.AccountId, transferId, amount, bookingDate, valueDate, EntryDirection.Debit));

        transfer.AssertInvariants();
        
        return transfer;
    }

    private void AssertInvariants()
    {
        var distinctCurrencies = _entries
            .Select(e => e.Amount.Currency)
            .Distinct(StringComparer.Ordinal)
            .Count();

        Guard.That(distinctCurrencies == 1, "Entries must share the same currency.");
        
        var sum = _entries
            .Select(e => e.SignedAmount().Amount)
            .Sum();
        
        Guard.That(sum == 0m, "Transfer must be balanced (sum == 0).");
        
        Guard.That(_entries.All(e => e.Amount.Currency == Currency), "Transfer currency mismatch.");
    }
}