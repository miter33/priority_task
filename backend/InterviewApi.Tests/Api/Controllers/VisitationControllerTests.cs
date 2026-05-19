using InterviewApi.Api.Controllers;
using InterviewApi.Application.Common.Dtos;
using InterviewApi.Application.Visitations.Commands.CreateVisitation;
using InterviewApi.Application.Visitations.Queries.SearchVisitations;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InterviewApi.Tests.Api.Controllers;

public class VisitationControllerTests
{
    [Fact]
    public async Task Search_DispatchesQuery_AndReturnsOk()
    {
        SearchVisitationsQuery? captured = null;
        var sender = new StubSender()
            .Handle<SearchVisitationsQuery, IReadOnlyList<VisitationView>>(q =>
            {
                captured = q;
                return new List<VisitationView> { new() { Id = 1, CustomerName = "X", HotelName = "Y" } };
            });
        var ctrl = new VisitationController(sender);

        var result = await ctrl.Search(month: 1, year: 2024, hotelIds: new List<int> { 1, 2 }, onlyLoyal: true, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rows = Assert.IsAssignableFrom<IReadOnlyList<VisitationView>>(ok.Value);
        Assert.Single(rows);
        Assert.NotNull(captured);
        Assert.Equal(1, captured!.Month);
        Assert.Equal(2024, captured.Year);
        Assert.True(captured.OnlyLoyal);
        Assert.Equal(new[] { 1, 2 }, captured.HotelIds);
    }

    [Theory]
    [InlineData(0, 2024)]
    [InlineData(13, 2024)]
    [InlineData(1, 1800)]
    [InlineData(1, 3000)]
    public async Task Search_ReturnsBadRequest_OnInvalidRange(int month, int year)
    {
        var ctrl = new VisitationController(new StubSender());

        var result = await ctrl.Search(month, year, null, false, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ReturnsCreated_OnValidInput()
    {
        var sender = new StubSender()
            .Handle<CreateVisitationCommand, Visitation>(cmd => new Visitation
            {
                Id = 1, CustomerId = cmd.CustomerId, HotelId = cmd.HotelId, VisitDate = cmd.VisitDate
            });
        var ctrl = new VisitationController(sender);

        var result = await ctrl.Register(new CreateVisitationRequest
        {
            CustomerId = 1,
            HotelId = 1,
            VisitDate = new DateTime(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc)
        }, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_OnNullBody()
    {
        var ctrl = new VisitationController(new StubSender());

        var result = await ctrl.Register(null!, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
