using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Visitations.Commands.CreateVisitation;

public class CreateVisitationCommandHandler : IRequestHandler<CreateVisitationCommand, Visitation>
{
    private readonly IVisitationRepository _visitations;
    private readonly ICustomerRepository _customers;
    private readonly IHotelRepository _hotels;

    public CreateVisitationCommandHandler(
        IVisitationRepository visitations,
        ICustomerRepository customers,
        IHotelRepository hotels)
    {
        _visitations = visitations;
        _customers = customers;
        _hotels = hotels;
    }

    public Task<Visitation> Handle(CreateVisitationCommand request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrorBuilder();
        if (request.CustomerId <= 0) errors.Add("customerId", "customerId must be a positive integer.");
        else if (_customers.GetById(request.CustomerId) is null) errors.Add("customerId", $"Customer {request.CustomerId} does not exist.");
        if (request.HotelId <= 0) errors.Add("hotelId", "hotelId must be a positive integer.");
        else if (_hotels.GetById(request.HotelId) is null) errors.Add("hotelId", $"Hotel {request.HotelId} does not exist.");
        if (request.VisitDate == default) errors.Add("visitDate", "visitDate is required.");
        errors.ThrowIfAny();

        var existing = _visitations.GetAll().FirstOrDefault(v =>
            v.CustomerId == request.CustomerId &&
            v.HotelId == request.HotelId &&
            v.VisitDate.Date == request.VisitDate.Date);
        if (existing is not null)
        {
            throw new ConflictException(
                $"Customer {request.CustomerId} already has a visit at hotel {request.HotelId} on {request.VisitDate:yyyy-MM-dd}.");
        }

        var visit = new Visitation
        {
            CustomerId = request.CustomerId,
            HotelId = request.HotelId,
            VisitDate = request.VisitDate
        };
        return Task.FromResult(_visitations.Add(visit));
    }
}
