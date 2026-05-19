using InterviewApi.Api.Controllers;
using InterviewApi.Application.Hotels.Queries.GetHotelById;
using InterviewApi.Application.Hotels.Queries.ListHotels;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InterviewApi.Tests.Api.Controllers;

public class HotelControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var sender = new StubSender()
            .Handle<ListHotelsQuery, IReadOnlyList<Hotel>>(_ => new List<Hotel>
            {
                new() { Id = 1, Name = "Grand" },
                new() { Id = 2, Name = "Seaside" },
            });
        var ctrl = new HotelController(sender);

        var result = await ctrl.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var hotels = Assert.IsAssignableFrom<IReadOnlyList<Hotel>>(ok.Value);
        Assert.Equal(2, hotels.Count);
    }

    [Fact]
    public async Task GetHotel_DispatchesQuery_AndReturnsOk()
    {
        var sender = new StubSender()
            .Handle<GetHotelByIdQuery, Hotel>(q => new Hotel { Id = q.Id, Name = "Grand" });
        var ctrl = new HotelController(sender);

        var result = await ctrl.GetHotel(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Grand", ((Hotel)ok.Value!).Name);
    }
}
