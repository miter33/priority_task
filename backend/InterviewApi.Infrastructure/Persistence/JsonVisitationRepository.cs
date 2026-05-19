using System.Text.Json;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;
using InterviewApi.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Hosting;

namespace InterviewApi.Infrastructure.Persistence;

public class JsonVisitationRepository : IVisitationRepository
{
    private readonly List<Visitation> _visitations;
    private readonly object _lock = new();

    public JsonVisitationRepository(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "visitations.json");
        _visitations = LoadFromFile(path);
    }

    private static List<Visitation> LoadFromFile(string path)
    {
        if (!File.Exists(path)) return new List<Visitation>();
        var json = File.ReadAllText(path);
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<VisitationData>(json, opts);
        return data?.Visitations ?? new List<Visitation>();
    }

    public IReadOnlyList<Visitation> GetAll()
    {
        lock (_lock) return _visitations.ToList();
    }

    public Visitation Add(Visitation visitation)
    {
        lock (_lock)
        {
            visitation.Id = _visitations.Count == 0 ? 1 : _visitations.Max(v => v.Id) + 1;
            _visitations.Add(visitation);
            return visitation;
        }
    }
}
