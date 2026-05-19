using InterviewApi.Application.Visitations.Loyalty;
using InterviewApi.Domain.Entities;
using Xunit;

namespace InterviewApi.Tests.Application.Visitations;

public class LoyaltyAnalyzerTests
{
    private static Visitation V(int id, int customerId, int hotelId, DateTime date) =>
        new() { Id = id, CustomerId = customerId, HotelId = hotelId, VisitDate = date };

    [Fact]
    public void AllFourSundaysOfJan2024_AreLoyal()
    {
        var visits = new List<Visitation>
        {
            V(1, 1, 1, new DateTime(2024, 1, 7,  10, 0, 0, DateTimeKind.Utc)),
            V(2, 1, 1, new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc)),
            V(3, 1, 1, new DateTime(2024, 1, 21, 10, 0, 0, DateTimeKind.Utc)),
            V(4, 1, 1, new DateTime(2024, 1, 28, 10, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, 1, 2024);

        Assert.Contains((1, 1, DayOfWeek.Sunday), loyal);
    }

    [Fact]
    public void ThreeOfFourSaturdays_NotLoyal()
    {
        var visits = new List<Visitation>
        {
            V(1, 2, 2, new DateTime(2024, 2, 3,  14, 0, 0, DateTimeKind.Utc)),
            V(2, 2, 2, new DateTime(2024, 2, 10, 14, 0, 0, DateTimeKind.Utc)),
            V(3, 2, 2, new DateTime(2024, 2, 17, 14, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, 2, 2024);

        Assert.Empty(loyal);
    }

    [Fact]
    public void MonthWithFiveOccurrences_RequiresFiveDistinctVisits()
    {
        // March 2024 has 5 Sundays; four visits should NOT make it loyal.
        var visits = new List<Visitation>
        {
            V(1, 1, 1, new DateTime(2024, 3, 3,  10, 0, 0, DateTimeKind.Utc)),
            V(2, 1, 1, new DateTime(2024, 3, 10, 10, 0, 0, DateTimeKind.Utc)),
            V(3, 1, 1, new DateTime(2024, 3, 17, 10, 0, 0, DateTimeKind.Utc)),
            V(4, 1, 1, new DateTime(2024, 3, 24, 10, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, 3, 2024);

        Assert.DoesNotContain((1, 1, DayOfWeek.Sunday), loyal);
    }

    [Fact]
    public void MonthWithFiveOccurrences_AllFive_IsLoyal()
    {
        var visits = new List<Visitation>
        {
            V(1, 1, 1, new DateTime(2024, 3, 3,  10, 0, 0, DateTimeKind.Utc)),
            V(2, 1, 1, new DateTime(2024, 3, 10, 10, 0, 0, DateTimeKind.Utc)),
            V(3, 1, 1, new DateTime(2024, 3, 17, 10, 0, 0, DateTimeKind.Utc)),
            V(4, 1, 1, new DateTime(2024, 3, 24, 10, 0, 0, DateTimeKind.Utc)),
            V(5, 1, 1, new DateTime(2024, 3, 31, 10, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, 3, 2024);

        Assert.Contains((1, 1, DayOfWeek.Sunday), loyal);
    }

    [Fact]
    public void TwoVisitsSameDay_CountAsOneOccurrence()
    {
        var visits = new List<Visitation>
        {
            V(1, 1, 1, new DateTime(2024, 1, 7,  10, 0, 0, DateTimeKind.Utc)),
            V(2, 1, 1, new DateTime(2024, 1, 7,  18, 0, 0, DateTimeKind.Utc)),
            V(3, 1, 1, new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc)),
            V(4, 1, 1, new DateTime(2024, 1, 21, 10, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, 1, 2024);

        Assert.DoesNotContain((1, 1, DayOfWeek.Sunday), loyal);
    }

    [Fact]
    public void WithoutMonthFilter_ComputesLoyaltyAcrossAllBuckets()
    {
        // Loyal pattern only exists in Jan 2024.
        var visits = new List<Visitation>
        {
            V(1, 1, 1, new DateTime(2024, 1, 7,  10, 0, 0, DateTimeKind.Utc)),
            V(2, 1, 1, new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc)),
            V(3, 1, 1, new DateTime(2024, 1, 21, 10, 0, 0, DateTimeKind.Utc)),
            V(4, 1, 1, new DateTime(2024, 1, 28, 10, 0, 0, DateTimeKind.Utc)),
            V(5, 2, 2, new DateTime(2024, 2, 3,  14, 0, 0, DateTimeKind.Utc)),
        };

        var loyal = LoyaltyAnalyzer.ComputeLoyalKeys(visits, month: null, year: null);

        Assert.Contains((1, 1, DayOfWeek.Sunday), loyal);
        Assert.DoesNotContain((2, 2, DayOfWeek.Saturday), loyal);
    }
}
