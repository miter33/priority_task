using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Visitations.Loyalty;

/// <summary>
/// Pure business rule that determines which (customer, hotel, weekday)
/// triples constitute a "loyal" pattern for a given month.
/// A pattern is loyal when the customer visits the same hotel on every
/// occurrence of a weekday within the month.
/// </summary>
public static class LoyaltyAnalyzer
{
    public static HashSet<(int customerId, int hotelId, DayOfWeek dow)> ComputeLoyalKeys(
        IReadOnlyList<Visitation> all, int? month, int? year)
    {
        var loyal = new HashSet<(int, int, DayOfWeek)>();

        IEnumerable<IGrouping<(int year, int month), Visitation>> buckets;
        if (month.HasValue && year.HasValue)
        {
            buckets = all
                .Where(v => v.VisitDate.Month == month.Value && v.VisitDate.Year == year.Value)
                .GroupBy(v => (v.VisitDate.Year, v.VisitDate.Month));
        }
        else
        {
            buckets = all.GroupBy(v => (v.VisitDate.Year, v.VisitDate.Month));
        }

        foreach (var bucket in buckets)
        {
            var (by, bm) = (bucket.Key.year, bucket.Key.month);
            var weekdayCounts = WeekdayOccurrencesInMonth(by, bm);

            var grouped = bucket.GroupBy(v => (v.CustomerId, v.HotelId, v.VisitDate.DayOfWeek));

            foreach (var g in grouped)
            {
                var distinctDays = g.Select(v => v.VisitDate.Date).Distinct().Count();
                if (distinctDays >= weekdayCounts[g.Key.DayOfWeek])
                {
                    loyal.Add((g.Key.CustomerId, g.Key.HotelId, g.Key.DayOfWeek));
                }
            }
        }

        return loyal;
    }

    private static Dictionary<DayOfWeek, int> WeekdayOccurrencesInMonth(int year, int month)
    {
        var counts = Enum.GetValues<DayOfWeek>().ToDictionary(d => d, _ => 0);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        for (int d = 1; d <= daysInMonth; d++)
        {
            counts[new DateTime(year, month, d).DayOfWeek]++;
        }
        return counts;
    }
}
