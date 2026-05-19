using InterviewApi.Domain.Entities;
using InterviewApi.Infrastructure.Persistence;
using Xunit;

namespace InterviewApi.Tests.Infrastructure.Persistence;

public class JsonCustomerRepositoryTests
{
    [Fact]
    public void GetAll_LoadsCustomersFromJsonFile()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv());

        var customers = repo.GetAll();

        Assert.Equal(2, customers.Count);
        Assert.Equal("John Doe", customers[0].Name);
    }

    [Fact]
    public void GetById_ReturnsCustomer_WhenExists()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv());

        var customer = repo.GetById(1);

        Assert.NotNull(customer);
        Assert.Equal("John Doe", customer!.Name);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenMissing()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv());

        Assert.Null(repo.GetById(999));
    }

    [Fact]
    public void Add_AssignsNextId_AndStoresCustomer()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv());

        var created = repo.Add(new Customer
        {
            Name = "New",
            Email = "new@example.com",
            RegistrationDate = DateTime.UtcNow,
        });

        Assert.Equal(3, created.Id);
        Assert.NotNull(repo.GetById(3));
    }

    [Fact]
    public void Add_StartsAtIdOne_WhenSourceFileEmpty()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv(customersJson: """{ "customers": [] }"""));

        var created = repo.Add(new Customer { Name = "Solo", Email = "solo@example.com" });

        Assert.Equal(1, created.Id);
    }

    [Fact]
    public void GetAll_ReturnsSnapshot_NotLiveCollection()
    {
        var repo = new JsonCustomerRepository(TestHelpers.MakeEnv());

        var snapshot = repo.GetAll();
        repo.Add(new Customer { Name = "X", Email = "x@x.com" });

        Assert.Equal(2, snapshot.Count);
        Assert.Equal(3, repo.GetAll().Count);
    }
}
