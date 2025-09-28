using System.Diagnostics;

namespace CoreLedger.Domain.Accounts;

[DebuggerDisplay("Acc:{AccountId} {Currency} [{Status}]")]
public sealed class Account
{
    private Account() { }
    
    public Guid AccountId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Currency { get; private set; } = "RUB";
    public AccountStatus Status { get; private set; }

    public Account(Guid accountId, Guid customerId, string currency, AccountStatus status = AccountStatus.Active)
    {
        AccountId = accountId;
        CustomerId = customerId;
        Currency = currency;
        Status = status;
    }

    public bool IsActive => Status == AccountStatus.Active;
}