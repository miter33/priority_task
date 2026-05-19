using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Common.Interfaces;

public interface IHotelRepository
{
    IReadOnlyList<Hotel> GetAll();
    Hotel? GetById(int id);
}
