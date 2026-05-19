using InterviewApi.Api.Controllers;
using InterviewApi.Application.Common.Dtos;
using InterviewApi.Application.Customers.Commands.CreateCustomer;
using InterviewApi.Application.Customers.Queries.GetCustomerById;
using InterviewApi.Application.Customers.Queries.ListCustomers;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InterviewApi.Tests.Api.Controllers;

public class CustomerControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithSenderResult()
    {
        var sender = new StubSender()
            .Handle<ListCustomersQuery, IReadOnlyList<Customer>>(_ => new List<Customer>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" },
            });
        var ctrl = new CustomerController(sender);

        var result = await ctrl.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<Customer>>(ok.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetCustomer_ReturnsOk_AndDispatchesQuery()
    {
        var sender = new StubSender()
            .Handle<GetCustomerByIdQuery, Customer>(q => new Customer { Id = q.Id, Name = "John" });
        var ctrl = new CustomerController(sender);

        var result = await ctrl.GetCustomer(7, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var customer = Assert.IsType<Customer>(ok.Value);
        Assert.Equal(7, customer.Id);
        Assert.Single(sender.SentRequests);
        Assert.IsType<GetCustomerByIdQuery>(sender.SentRequests[0]);
    }

    [Fact]
    public async Task AddCustomer_ReturnsCreated_OnValidInput()
    {
        var sender = new StubSender()
            .Handle<CreateCustomerCommand, Customer>(cmd => new Customer
            {
                Id = 9, Name = cmd.Name, Email = cmd.Email
            });
        var ctrl = new CustomerController(sender);

        var result = await ctrl.AddCustomer(new CreateCustomerRequest { Name = "New", Email = "new@example.com" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var customer = Assert.IsType<Customer>(created.Value);
        Assert.Equal(9, customer.Id);
        Assert.Equal("New", customer.Name);
    }

    [Fact]
    public async Task AddCustomer_ReturnsBadRequest_OnNullBody()
    {
        var ctrl = new CustomerController(new StubSender());

        var result = await ctrl.AddCustomer(null!, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
