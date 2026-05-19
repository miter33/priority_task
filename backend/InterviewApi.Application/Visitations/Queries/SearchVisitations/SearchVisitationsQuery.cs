using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Dtos;

namespace InterviewApi.Application.Visitations.Queries.SearchVisitations;

public record SearchVisitationsQuery(
    int? Month,
    int? Year,
    IReadOnlyCollection<int>? HotelIds,
    bool OnlyLoyal) : IRequest<IReadOnlyList<VisitationView>>;
