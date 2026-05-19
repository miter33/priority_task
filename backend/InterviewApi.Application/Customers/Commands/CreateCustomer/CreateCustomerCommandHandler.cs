using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;
using EmailAddressAttribute = System.ComponentModel.DataAnnotations.EmailAddressAttribute;

namespace InterviewApi.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Customer>
{
    private readonly ICustomerRepository _customers;

    public CreateCustomerCommandHandler(ICustomerRepository customers) => _customers = customers;

    public Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Name)) errors.Add("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Email)) errors.Add("Email is required.");
        else if (!new EmailAddressAttribute().IsValid(request.Email)) errors.Add("Email is invalid.");
        if (errors.Count > 0) throw new ValidationException(errors);

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            RegistrationDate = DateTime.UtcNow,
            TotalPurchases = 0
        };
        return Task.FromResult(_customers.Add(customer));
    }
}
