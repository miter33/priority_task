using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Customers.Queries.ListCustomers;

public class ListCustomersQueryHandler : IRequestHandler<ListCustomersQuery, IReadOnlyList<Customer>>
{
    private readonly ICustomerRepository _customers;

    public ListCustomersQueryHandler(ICustomerRepository customers) => _customers = customers;

    public Task<IReadOnlyList<Customer>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
        => Task.FromResult(_customers.GetAll());
}
