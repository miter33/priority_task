using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Hotels.Queries.GetHotelById;

public record GetHotelByIdQuery(int Id) : IRequest<Hotel>;
