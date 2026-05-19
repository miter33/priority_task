using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Hotels.Queries.ListHotels;

public class ListHotelsQueryHandler : IRequestHandler<ListHotelsQuery, IReadOnlyList<Hotel>>
{
    private readonly IHotelRepository _hotels;

    public ListHotelsQueryHandler(IHotelRepository hotels) => _hotels = hotels;

    public Task<IReadOnlyList<Hotel>> Handle(ListHotelsQuery request, CancellationToken cancellationToken)
        => Task.FromResult(_hotels.GetAll());
}
