using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace InterviewApi.Tests;

internal class StubEnv : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "InterviewApi.Tests";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Test";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = string.Empty;
}

internal static class TestHelpers
{
    public const string CustomersJson = """
{
  "customers": [
    { "id": 1, "name": "John Doe",  "email": "john@example.com", "registrationDate": "2024-01-15T10:30:00Z", "totalPurchases": 15 },
    { "id": 2, "name": "Jane Smith","email": "jane@example.com", "registrationDate": "2024-02-20T14:45:00Z", "totalPurchases": 8 }
  ]
}
""";

    public const string HotelsJson = """
{
  "hotels": [
    { "id": 1, "name": "Grand Hotel",     "location": "Downtown",     "rating": 4.5, "description": "" },
    { "id": 2, "name": "Seaside Resort",  "location": "Beachfront",   "rating": 4.2, "description": "" },
    { "id": 3, "name": "Mountain Lodge",  "location": "Mountain View","rating": 4.0, "description": "" }
  ]
}
""";

    // John Doe: every Sunday of Jan 2024 (4 Sundays) at Grand Hotel -> loyal
    // Jane Smith: 3 of 4 Saturdays of Feb 2024 at Seaside Resort -> NOT loyal
    public const string VisitationsJson = """
{
  "visitations": [
    { "id": 1, "customerId": 1, "hotelId": 1, "visitDate": "2024-01-07T10:00:00Z" },
    { "id": 2, "customerId": 1, "hotelId": 1, "visitDate": "2024-01-14T10:00:00Z" },
    { "id": 3, "customerId": 1, "hotelId": 1, "visitDate": "2024-01-21T10:00:00Z" },
    { "id": 4, "customerId": 1, "hotelId": 1, "visitDate": "2024-01-28T10:00:00Z" },
    { "id": 5, "customerId": 2, "hotelId": 2, "visitDate": "2024-02-03T14:00:00Z" },
    { "id": 6, "customerId": 2, "hotelId": 2, "visitDate": "2024-02-10T14:00:00Z" },
    { "id": 7, "customerId": 2, "hotelId": 2, "visitDate": "2024-02-17T14:00:00Z" }
  ]
}
""";

    public static string MakeTempDataDir(
        string? customersJson = null,
        string? hotelsJson = null,
        string? visitationsJson = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "InterviewApiTests_" + Guid.NewGuid().ToString("N"));
        var data = Path.Combine(root, "Data");
        Directory.CreateDirectory(data);

        File.WriteAllText(Path.Combine(data, "customers.json"), customersJson ?? CustomersJson);
        File.WriteAllText(Path.Combine(data, "hotels.json"), hotelsJson ?? HotelsJson);
        File.WriteAllText(Path.Combine(data, "visitations.json"), visitationsJson ?? VisitationsJson);

        return root;
    }

    public static StubEnv MakeEnv(
        string? customersJson = null,
        string? hotelsJson = null,
        string? visitationsJson = null) =>
        new StubEnv { ContentRootPath = MakeTempDataDir(customersJson, hotelsJson, visitationsJson) };
}
