using System.Diagnostics;

namespace CoreLedger.Domain.Accounts;

[DebuggerDisplay("Acc:{AccountId} {Currency} [{Status}]")]
public sealed class Account
{
    public Guid AccountId { get; }
    public Guid CustomerId { get; }
    public string Currency { get; }
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