using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Hotels.Queries.GetHotelById;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, Hotel>
{
    private readonly IHotelRepository _hotels;

    public GetHotelByIdQueryHandler(IHotelRepository hotels) => _hotels = hotels;

    public Task<Hotel> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = _hotels.GetById(request.Id)
            ?? throw new NotFoundException(nameof(Hotel), request.Id);
        return Task.FromResult(hotel);
    }
}
