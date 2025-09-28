using CoreLedger.Domain.Errors;
using CoreLedger.Domain.Ledger;
using FluentAssertions;

namespace CoreLedger.Tests.Domain;

public class LedgerEntryTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenValueDateIsEarlierThanBookingDate()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var amount = new Money(10.0000m, "RUB");
        var booking = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var value = booking.AddDays(-1);

        // Act
        var act = () =>
        {
            _ = new LedgerEntry(
                entryId: Guid.NewGuid(),
                accountId: accountId,
                transferId: transferId,
                amount: amount,
                bookingDate: booking,
                valueDate: value,
                direction: EntryDirection.Debit
            );
        };

        // Assert
        act.Should().Throw<InvariantViolation>()
            .WithMessage("*ValueDate must be >= BookingDate*");
    }
}