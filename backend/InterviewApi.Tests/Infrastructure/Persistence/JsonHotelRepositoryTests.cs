using InterviewApi.Infrastructure.Persistence;
using Xunit;

namespace InterviewApi.Tests.Infrastructure.Persistence;

public class JsonHotelRepositoryTests
{
    [Fact]
    public void GetAll_LoadsHotelsFromJsonFile()
    {
        var repo = new JsonHotelRepository(TestHelpers.MakeEnv());

        var hotels = repo.GetAll();

        Assert.Equal(3, hotels.Count);
        Assert.Contains(hotels, h => h.Name == "Grand Hotel");
    }

    [Fact]
    public void GetById_ReturnsHotel_WhenExists()
    {
        var repo = new JsonHotelRepository(TestHelpers.MakeEnv());

        Assert.Equal("Seaside Resort", repo.GetById(2)!.Name);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenMissing()
    {
        var repo = new JsonHotelRepository(TestHelpers.MakeEnv());

        Assert.Null(repo.GetById(404));
    }

    [Fact]
    public void GetAll_ReturnsEmpty_WhenFileMissing()
    {
        var env = new StubEnv { ContentRootPath = Path.Combine(Path.GetTempPath(), "no-data-" + Guid.NewGuid()) };

        var repo = new JsonHotelRepository(env);

        Assert.Empty(repo.GetAll());
    }
}
