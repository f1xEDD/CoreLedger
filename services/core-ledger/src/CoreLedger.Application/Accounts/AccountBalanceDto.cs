namespace CoreLedger.Application.Accounts;

public sealed record AccountBalanceDto(
    Guid AccountId,
    decimal Balance,
    string Currency);
