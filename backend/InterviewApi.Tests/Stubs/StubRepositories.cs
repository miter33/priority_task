using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Tests.Stubs;

internal class StubCustomerRepository : ICustomerRepository
{
    public List<Customer> Store { get; } = new();
    public int AddCalls { get; private set; }
    public IReadOnlyList<Customer> GetAll() => Store;
    public Customer? GetById(int id) => Store.FirstOrDefault(c => c.Id == id);
    public Customer Add(Customer customer)
    {
        AddCalls++;
        customer.Id = Store.Count == 0 ? 1 : Store.Max(c => c.Id) + 1;
        Store.Add(customer);
        return customer;
    }
}

internal class StubHotelRepository : IHotelRepository
{
    public List<Hotel> Store { get; } = new();
    public IReadOnlyList<Hotel> GetAll() => Store;
    public Hotel? GetById(int id) => Store.FirstOrDefault(h => h.Id == id);
}

internal class StubVisitationRepository : IVisitationRepository
{
    public List<Visitation> Store { get; } = new();
    public IReadOnlyList<Visitation> GetAll() => Store;
    public Visitation Add(Visitation visitation)
    {
        visitation.Id = Store.Count == 0 ? 1 : Store.Max(v => v.Id) + 1;
        Store.Add(visitation);
        return visitation;
    }
}
