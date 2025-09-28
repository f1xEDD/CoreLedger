using CoreLedger.Domain.Accounts;
using CoreLedger.Domain.Errors;
using CoreLedger.Domain.Ledger;

namespace CoreLedger.Domain.Transfers;

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
        if (from.AccountId == to.AccountId)
        {
            throw new InvalidOperationError("From and To must be different accounts.");
        }

        if (!from.IsActive || !to.IsActive)
        {
            throw new InvalidOperationError("Both accounts must be active.");
        }

        if (!string.Equals(from.Currency, to.Currency, StringComparison.Ordinal))
        {
            throw new CurrencyMismatch();
        }
        
        amount.EnsureSameCurrency(new Money(0m, from.Currency));

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

        if (distinctCurrencies != 1)
        {
            throw new InvariantViolation("Entries must share the same currency.");
        }
        
        var sum = _entries
            .Select(e => e.SignedAmount().Amount)
            .Sum();
        
        if (sum != 0m)
        {
            throw new InvariantViolation("Transfer must be balanced (sum == 0).");
        }
    }
}