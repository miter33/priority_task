using InterviewApi.Application.Visitations.Queries.SearchVisitations;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Xunit;

namespace InterviewApi.Tests.Application.Visitations;

public class SearchVisitationsQueryHandlerTests
{
    private static (SearchVisitationsQueryHandler handler,
                    StubVisitationRepository visits,
                    StubCustomerRepository customers,
                    StubHotelRepository hotels) Build()
    {
        var visits = new StubVisitationRepository();
        var customers = new StubCustomerRepository();
        var hotels = new StubHotelRepository();

        customers.Store.AddRange(new[]
        {
            new Customer { Id = 1, Name = "John Doe" },
            new Customer { Id = 2, Name = "Jane Smith" },
        });
        hotels.Store.AddRange(new[]
        {
            new Hotel { Id = 1, Name = "Grand Hotel" },
            new Hotel { Id = 2, Name = "Seaside Resort" },
        });
        visits.Store.AddRange(new[]
        {
            new Visitation { Id = 1, CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 1, 7,  10, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 2, CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 3, CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 1, 21, 10, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 4, CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 1, 28, 10, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 5, CustomerId = 2, HotelId = 2, VisitDate = new DateTime(2024, 2, 3,  14, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 6, CustomerId = 2, HotelId = 2, VisitDate = new DateTime(2024, 2, 10, 14, 0, 0, DateTimeKind.Utc) },
            new Visitation { Id = 7, CustomerId = 2, HotelId = 2, VisitDate = new DateTime(2024, 2, 17, 14, 0, 0, DateTimeKind.Utc) },
        });

        return (new SearchVisitationsQueryHandler(visits, customers, hotels), visits, customers, hotels);
    }

    [Fact]
    public async Task FiltersByMonthAndYear()
    {
        var (handler, _, _, _) = Build();

        var rows = (await handler.Handle(new SearchVisitationsQuery(1, 2024, null, false), CancellationToken.None)).Items;

        Assert.Equal(4, rows.Count);
        Assert.All(rows, r => Assert.Equal(1, r.VisitDate.Month));
    }

    [Fact]
    public async Task FiltersByHotelIds()
    {
        var (handler, _, _, _) = Build();

        var rows = (await handler.Handle(new SearchVisitationsQuery(null, null, new[] { 2 }, false), CancellationToken.None)).Items;

        Assert.NotEmpty(rows);
        Assert.All(rows, r => Assert.Equal(2, r.HotelId));
    }

    [Fact]
    public async Task EnrichesRowsWithCustomerAndHotelNames()
    {
        var (handler, _, _, _) = Build();

        var rows = (await handler.Handle(new SearchVisitationsQuery(1, 2024, null, false), CancellationToken.None)).Items;

        var first = rows.First();
        Assert.Equal("John Doe", first.CustomerName);
        Assert.Equal("Grand Hotel", first.HotelName);
    }

    [Fact]
    public async Task MarksLoyalRowsCorrectly_ForJan2024()
    {
        var (handler, _, _, _) = Build();

        var rows = (await handler.Handle(new SearchVisitationsQuery(1, 2024, null, false), CancellationToken.None)).Items;

        Assert.All(rows, r => Assert.True(r.IsLoyal));
    }

    [Fact]
    public async Task DoesNotMarkLoyal_WhenWeekdayCoveragePartial()
    {
        var (handler, _, _, _) = Build();

        var rows = (await handler.Handle(new SearchVisitationsQuery(2, 2024, null, false), CancellationToken.None)).Items;

        Assert.All(rows, r => Assert.False(r.IsLoyal));
    }

    [Fact]
    public async Task OnlyLoyal_FiltersNonLoyalRows()
    {
        var (handler, _, _, _) = Build();

        var result = await handler.Handle(new SearchVisitationsQuery(2, 2024, null, true), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task OnlyLoyal_ReturnsLoyalRowsWhenPresent()
    {
        var (handler, _, _, _) = Build();

        var result = await handler.Handle(new SearchVisitationsQuery(1, 2024, null, true), CancellationToken.None);

        Assert.Equal(4, result.Items.Count);
        Assert.Equal(4, result.Total);
        Assert.All(result.Items, r => Assert.True(r.IsLoyal));
    }

    [Fact]
    public async Task Pagination_ReturnsRequestedPage_AndKeepsTotal()
    {
        var (handler, _, _, _) = Build();

        var result = await handler.Handle(
            new SearchVisitationsQuery(null, null, null, false, Page: 2, PageSize: 3),
            CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(7, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    public async Task Pagination_BeyondEnd_ReturnsEmptyItemsButTotalIntact()
    {
        var (handler, _, _, _) = Build();

        var result = await handler.Handle(
            new SearchVisitationsQuery(null, null, null, false, Page: 99, PageSize: 3),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(7, result.Total);
    }

    [Fact]
    public async Task Pagination_PageSizeClampedToMax()
    {
        var (handler, _, _, _) = Build();

        var result = await handler.Handle(
            new SearchVisitationsQuery(null, null, null, false, Page: 1, PageSize: 10_000),
            CancellationToken.None);

        Assert.Equal(200, result.PageSize);
        Assert.Equal(7, result.Total);
    }
}
