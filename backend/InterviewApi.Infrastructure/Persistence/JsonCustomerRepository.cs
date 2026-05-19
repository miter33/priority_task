using System.Text.Json;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;
using InterviewApi.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Hosting;

namespace InterviewApi.Infrastructure.Persistence;

public class JsonCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers;
    private readonly object _lock = new();

    public JsonCustomerRepository(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "customers.json");
        _customers = LoadFromFile(path);
    }

    private static List<Customer> LoadFromFile(string path)
    {
        if (!File.Exists(path)) return new List<Customer>();
        var json = File.ReadAllText(path);
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<CustomerData>(json, opts);
        return data?.Customers ?? new List<Customer>();
    }

    public IReadOnlyList<Customer> GetAll()
    {
        lock (_lock) return _customers.ToList();
    }

    public Customer? GetById(int id)
    {
        lock (_lock) return _customers.FirstOrDefault(c => c.Id == id);
    }

    public Customer Add(Customer customer)
    {
        lock (_lock)
        {
            customer.Id = _customers.Count == 0 ? 1 : _customers.Max(c => c.Id) + 1;
            _customers.Add(customer);
            return customer;
        }
    }
}
