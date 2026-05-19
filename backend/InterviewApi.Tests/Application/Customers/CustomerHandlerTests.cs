using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Customers.Commands.CreateCustomer;
using InterviewApi.Application.Customers.Queries.GetCustomerById;
using InterviewApi.Application.Customers.Queries.ListCustomers;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Xunit;

namespace InterviewApi.Tests.Application.Customers;

public class CustomerHandlerTests
{
    private static StubCustomerRepository Repo()
    {
        var repo = new StubCustomerRepository();
        repo.Store.Add(new Customer { Id = 1, Name = "John", Email = "john@example.com" });
        repo.Store.Add(new Customer { Id = 2, Name = "Jane", Email = "jane@example.com" });
        return repo;
    }

    [Fact]
    public async Task GetCustomerById_ReturnsCustomer()
    {
        var handler = new GetCustomerByIdQueryHandler(Repo());

        var c = await handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

        Assert.Equal("John", c.Name);
    }

    [Fact]
    public async Task GetCustomerById_Throws_WhenMissing()
    {
        var handler = new GetCustomerByIdQueryHandler(Repo());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetCustomerByIdQuery(99), CancellationToken.None));
    }

    [Fact]
    public async Task ListCustomers_ReturnsAll()
    {
        var handler = new ListCustomersQueryHandler(Repo());

        var customers = await handler.Handle(new ListCustomersQuery(), CancellationToken.None);

        Assert.Equal(2, customers.Count);
    }

    [Fact]
    public async Task CreateCustomer_AddsAndAssignsId()
    {
        var repo = Repo();
        var handler = new CreateCustomerCommandHandler(repo);

        var created = await handler.Handle(new CreateCustomerCommand("New", "new@example.com"), CancellationToken.None);

        Assert.Equal(3, created.Id);
        Assert.Equal(1, repo.AddCalls);
    }

    [Theory]
    [InlineData("", "x@x.com")]
    [InlineData("Name", "")]
    [InlineData("Name", "not-an-email")]
    [InlineData("   ", "x@x.com")]
    public async Task CreateCustomer_Throws_OnInvalidInput(string name, string email)
    {
        var handler = new CreateCustomerCommandHandler(Repo());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateCustomerCommand(name, email), CancellationToken.None));
    }
}
