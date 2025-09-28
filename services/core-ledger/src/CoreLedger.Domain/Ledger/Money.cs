using System.Diagnostics;
using CoreLedger.Domain.Errors;

namespace CoreLedger.Domain.Ledger;

[DebuggerDisplay("{Amount} {Currency}")]
public readonly record struct Money
{
    public decimal Amount { get; }
    
    public string Currency { get; }

    public static readonly string DefaultCurrency = "RUB";

    public Money(decimal amount, string? currency = null)
    {
        Currency = currency ?? DefaultCurrency;
        Amount = decimal.Round(amount, 4,  MidpointRounding.ToEven);
    }
    
    public static Money Zero(string? currency = null) => new(0m, currency ?? DefaultCurrency);

    public void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new CurrencyMismatch();
        }
    }

    public static Money operator +(Money a, Money b)
    {
        a.EnsureSameCurrency(b);
        
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator -(Money a, Money b)
    {
        a.EnsureSameCurrency(b);
        
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal factor)
    {
        return new Money(decimal.Round(a.Amount * factor, 4, MidpointRounding.ToEven), a.Currency);
    }
}