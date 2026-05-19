using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Hotels.Queries.GetHotelById;
using InterviewApi.Application.Hotels.Queries.ListHotels;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Xunit;

namespace InterviewApi.Tests.Application.Hotels;

public class HotelHandlerTests
{
    private static StubHotelRepository Repo()
    {
        var repo = new StubHotelRepository();
        repo.Store.Add(new Hotel { Id = 1, Name = "Grand" });
        repo.Store.Add(new Hotel { Id = 2, Name = "Seaside" });
        return repo;
    }

    [Fact]
    public async Task ListHotels_ReturnsAll()
    {
        var hotels = await new ListHotelsQueryHandler(Repo()).Handle(new ListHotelsQuery(), CancellationToken.None);

        Assert.Equal(2, hotels.Count);
    }

    [Fact]
    public async Task GetHotelById_ReturnsHotel()
    {
        var hotel = await new GetHotelByIdQueryHandler(Repo()).Handle(new GetHotelByIdQuery(1), CancellationToken.None);

        Assert.Equal("Grand", hotel.Name);
    }

    [Fact]
    public async Task GetHotelById_Throws_WhenMissing()
    {
        var handler = new GetHotelByIdQueryHandler(Repo());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetHotelByIdQuery(99), CancellationToken.None));
    }
}
