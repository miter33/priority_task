using System.Text.Json;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Domain.Entities;
using InterviewApi.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Hosting;

namespace InterviewApi.Infrastructure.Persistence;

public class JsonHotelRepository : IHotelRepository
{
    private readonly List<Hotel> _hotels;

    public JsonHotelRepository(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "hotels.json");
        _hotels = LoadFromFile(path);
    }

    private static List<Hotel> LoadFromFile(string path)
    {
        if (!File.Exists(path)) return new List<Hotel>();
        var json = File.ReadAllText(path);
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<HotelData>(json, opts);
        return data?.Hotels ?? new List<Hotel>();
    }

    public IReadOnlyList<Hotel> GetAll() => _hotels.AsReadOnly();

    public Hotel? GetById(int id) => _hotels.FirstOrDefault(h => h.Id == id);
}
