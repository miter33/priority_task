using System.Text.Json.Serialization;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Infrastructure.Persistence.Models;

internal class CustomerData
{
    [JsonPropertyName("customers")]
    public List<Customer> Customers { get; set; } = new();
}
