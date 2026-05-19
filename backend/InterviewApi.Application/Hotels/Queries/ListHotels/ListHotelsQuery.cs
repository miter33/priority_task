using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Hotels.Queries.ListHotels;

public record ListHotelsQuery : IRequest<IReadOnlyList<Hotel>>;
