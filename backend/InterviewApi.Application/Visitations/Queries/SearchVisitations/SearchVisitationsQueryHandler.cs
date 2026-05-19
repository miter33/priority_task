using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Dtos;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Application.Visitations.Loyalty;

namespace InterviewApi.Application.Visitations.Queries.SearchVisitations;

public class SearchVisitationsQueryHandler
    : IRequestHandler<SearchVisitationsQuery, PagedResult<VisitationView>>
{
    private const int MaxPageSize = 200;

    private readonly IVisitationRepository _visitations;
    private readonly ICustomerRepository _customers;
    private readonly IHotelRepository _hotels;

    public SearchVisitationsQueryHandler(
        IVisitationRepository visitations,
        ICustomerRepository customers,
        IHotelRepository hotels)
    {
        _visitations = visitations;
        _customers = customers;
        _hotels = hotels;
    }

    public Task<PagedResult<VisitationView>> Handle(
        SearchVisitationsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var all = _visitations.GetAll();

        var filtered = all.AsEnumerable();
        if (request.Month.HasValue) filtered = filtered.Where(v => v.VisitDate.Month == request.Month.Value);
        if (request.Year.HasValue) filtered = filtered.Where(v => v.VisitDate.Year == request.Year.Value);
        if (request.HotelIds is { Count: > 0 }) filtered = filtered.Where(v => request.HotelIds.Contains(v.HotelId));

        var loyalKeys = LoyaltyAnalyzer.ComputeLoyalKeys(all, request.Month, request.Year);
        var customers = _customers.GetAll().ToDictionary(c => c.Id);
        var hotels = _hotels.GetAll().ToDictionary(h => h.Id);

        var enriched = filtered
            .OrderBy(v => v.VisitDate)
            .ThenBy(v => v.Id)
            .Select(v => new VisitationView
            {
                Id = v.Id,
                CustomerId = v.CustomerId,
                CustomerName = customers.TryGetValue(v.CustomerId, out var c) ? c.Name : $"#{v.CustomerId}",
                HotelId = v.HotelId,
                HotelName = hotels.TryGetValue(v.HotelId, out var h) ? h.Name : $"#{v.HotelId}",
                VisitDate = v.VisitDate,
                IsLoyal = loyalKeys.Contains((v.CustomerId, v.HotelId, v.VisitDate.DayOfWeek))
            });

        if (request.OnlyLoyal) enriched = enriched.Where(v => v.IsLoyal);

        var materialized = enriched.ToList();
        var total = materialized.Count;
        var pageItems = materialized
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<VisitationView>
        {
            Items = pageItems,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
