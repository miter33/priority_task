using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Visitations.Commands.CreateVisitation;

public record CreateVisitationCommand(int CustomerId, int HotelId, DateTime VisitDate) : IRequest<Visitation>;
