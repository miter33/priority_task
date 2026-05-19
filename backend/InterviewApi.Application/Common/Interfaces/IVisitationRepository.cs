using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Common.Interfaces;

public interface IVisitationRepository
{
    IReadOnlyList<Visitation> GetAll();
    Visitation Add(Visitation visitation);
}
