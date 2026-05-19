using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int Id) : IRequest<Customer>;
