using CoreLedger.Domain.Time;
using FluentAssertions;

namespace CoreLedger.Tests.Domain;

public class DateFactoryTests
{
    [Fact]
    public void DateFactory_TodayPlusDays_ShouldUseProvider()
    {
        //Arrange
        var fixedUtc = new DateTime(2025, 09, 28, 12, 0, 0, DateTimeKind.Utc);
        var time = new TestTimeProvider(fixedUtc);

        //Act
        var today = DateFactory.Today(time);
        var tomorrow = DateFactory.TodayPlusDays(time, 1);

        //Assert
        today.Should().Be(new DateOnly(2025, 09, 28));
        tomorrow.Should().Be(new DateOnly(2025, 09, 29));
    }
}