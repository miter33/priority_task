using System.Text.Json.Serialization;
using InterviewApi.Domain.Entities;

namespace InterviewApi.Infrastructure.Persistence.Models;

internal class VisitationData
{
    [JsonPropertyName("visitations")]
    public List<Visitation> Visitations { get; set; } = new();
}
