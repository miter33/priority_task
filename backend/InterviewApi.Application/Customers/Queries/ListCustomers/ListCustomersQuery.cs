using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Customers.Queries.ListCustomers;

public record ListCustomersQuery : IRequest<IReadOnlyList<Customer>>;
