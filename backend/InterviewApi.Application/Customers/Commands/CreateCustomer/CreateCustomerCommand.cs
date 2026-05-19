using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(string Name, string Email) : IRequest<Customer>;
