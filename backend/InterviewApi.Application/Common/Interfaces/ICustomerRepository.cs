using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Common.Interfaces;

public interface ICustomerRepository
{
    IReadOnlyList<Customer> GetAll();
    Customer? GetById(int id);
    Customer Add(Customer customer);
}
