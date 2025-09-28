using CoreLedger.Domain.Errors;
using CoreLedger.Domain.Ledger;
using FluentAssertions;

namespace CoreLedger.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Add_ShouldThrow_WhenCurrenciesDiffer()
    {
        //Arrange
        var rub = new Money(10.1234m, "RUB");
        var usd = new Money(5.0000m, "USD");
        
        //Act
        var act = () => { _ = rub + usd; };
        
        //Assert
        act.Should().Throw<CurrencyMismatch>();
    }
    
    [Fact]
    public void Add_ShouldKeepCurrency_AndScale4()
    {
        // Arrange
        var a = new Money(10.1234m, "RUB");
        var b = new Money( 5.8766m, "RUB");

        // Act
        var sum = a + b;

        // Assert
        sum.Currency.Should().Be("RUB");
        ScaleOf(sum.Amount).Should().Be(4);
        sum.Amount.Should().Be(16.0000m);
    }

    [Fact]
    public void Subtract_ShouldRespectCurrency_AndScale4()
    {
        // Arrange
        var a = new Money(10.0000m, "RUB");
        var b = new Money( 3.3333m, "RUB");

        // Act
        var diff = a - b;

        // Assert
        diff.Currency.Should().Be("RUB");
        ScaleOf(diff.Amount).Should().Be(4);
        diff.Amount.Should().Be(6.6667m);
    }
    
    [Fact]
    public void Multiply_ShouldRoundToScale4_MidpointToEven()
    {
        // Arrange
        var one = new Money(1.0000m, "RUB");
        
        const decimal factorEven = 1.23445m;
        const decimal factorOdd = 1.23455m;

        // Act
        var midDown = one * factorEven;
        var midUp   = one * factorOdd;

        // Assert
        midDown.Amount.Should().Be(1.2344m);
        ScaleOf(midDown.Amount).Should().Be(4);

        midUp.Amount.Should().Be(1.2346m);
        ScaleOf(midUp.Amount).Should().Be(4);

        midDown.Currency.Should().Be("RUB");
        midUp.Currency.Should().Be("RUB");
    }
    
    [Fact]
    public void Equals_ShouldBeTrue_ForSameAmountAndCurrency()
    {
        // Arrange
        var a = new Money(10.12346m, "RUB");
        var b = new Money(10.1235m,  "RUB");
        var e = new Money(10.1234m, "USD");

        // Act + Assert
        a.Should().Be(b);
        (a == b).Should().BeTrue();
        
        a.Should().NotBe(e);
        (a != e).Should().BeTrue();
    }
    
    [Fact]
    public void Money_ToString_ShouldBeHumanReadable()
    {
        //Arrange
        var m = new Money(10.5000m, "RUB");
        
        //Act + Assert
        m.ToString().Should().Be("10.5 RUB");
    }
    
    private static int ScaleOf(decimal d)
    {
        var bits = decimal.GetBits(d);

        return (bits[3] >> 16) & 0xFF;
    }
}