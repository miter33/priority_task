using System.Text.Json.Serialization;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Infrastructure.Persistence.Models;

internal class HotelData
{
    [JsonPropertyName("hotels")]
    public List<Hotel> Hotels { get; set; } = new();
}
