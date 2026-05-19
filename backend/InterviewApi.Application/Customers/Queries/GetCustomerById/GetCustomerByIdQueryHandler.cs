using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Customer>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerByIdQueryHandler(ICustomerRepository customers) => _customers = customers;

    public Task<Customer> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = _customers.GetById(request.Id)
            ?? throw new NotFoundException(nameof(Customer), request.Id);
        return Task.FromResult(customer);
    }
}
