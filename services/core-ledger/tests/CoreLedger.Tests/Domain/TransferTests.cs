using CoreLedger.Domain.Accounts;
using CoreLedger.Domain.Errors;
using CoreLedger.Domain.Ledger;
using CoreLedger.Domain.Transfers;
using FluentAssertions;

namespace CoreLedger.Tests.Domain;

public class TransferTests
{
    private static Account ActiveAccountRub(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), customerId: Guid.NewGuid(), currency: "RUB", status: AccountStatus.Active);

    private static Account InactiveAccountRub() =>
        new(Guid.NewGuid(), customerId: Guid.NewGuid(), currency: "RUB", status: AccountStatus.Blocked);

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Fact]
    public void Create_ShouldProduceTwoBalancedEntries_WhenHappyPath()
    {
        // Arrange
        var from = ActiveAccountRub();
        var to = ActiveAccountRub();
        var amount = new Money(100.0000m, "RUB");
        var booking = Today;
        var value = Today;

        // Act
        var transfer = Transfer.Create(Guid.NewGuid(), from, to, amount, booking, value);

        // Assert
        transfer.Entries.Should().HaveCount(2);

        transfer.Entries
            .Select(e => e.Amount.Currency)
            .Distinct()
            .Should()
            .ContainSingle().Which.Should().Be("RUB");

        var signedSum = transfer.Entries.Select(e => e.SignedAmount().Amount).Sum();

        signedSum.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldThrow_WhenFromEqualsTo()
    {
        // Arrange
        var account = ActiveAccountRub();
        var amount = new Money(10.0000m, "RUB");

        // Act
        Action act = () => Transfer.Create(Guid.NewGuid(), account, account, amount, Today, Today);

        // Assert
        act.Should().Throw<InvalidOperationError>()
            .WithMessage("*From and To must be different*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenCurrenciesDiffer()
    {
        // Arrange
        var from = new Account(Guid.NewGuid(), Guid.NewGuid(), "RUB");
        var to = new Account(Guid.NewGuid(), Guid.NewGuid(), "USD");
        var amount = new Money(50.0000m, "RUB");

        // Act
        Action act = () => Transfer.Create(Guid.NewGuid(), from, to, amount, Today, Today);

        // Assert
        act.Should().Throw<CurrencyMismatch>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenAnyAccountIsNotActive()
    {
        // Arrange
        var from = InactiveAccountRub();
        var to = ActiveAccountRub();
        var amount = new Money(25.0000m, "RUB");

        // Act
        Action act = () => Transfer.Create(Guid.NewGuid(), from, to, amount, Today, Today);

        // Assert
        act.Should().Throw<InvalidOperationError>()
            .WithMessage("*must be active*");
    }
}