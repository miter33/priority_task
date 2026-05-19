using InterviewApi.Application.Common.Exceptions;
using InterviewApi.Application.Visitations.Commands.CreateVisitation;
using InterviewApi.Domain.Entities;
using InterviewApi.Tests.Stubs;
using Xunit;

namespace InterviewApi.Tests.Application.Visitations;

public class CreateVisitationCommandHandlerTests
{
    private static (CreateVisitationCommandHandler handler,
                    StubVisitationRepository visits) Build()
    {
        var visits = new StubVisitationRepository();
        var customers = new StubCustomerRepository();
        var hotels = new StubHotelRepository();
        customers.Store.Add(new Customer { Id = 1, Name = "John" });
        hotels.Store.Add(new Hotel { Id = 1, Name = "Grand" });
        return (new CreateVisitationCommandHandler(visits, customers, hotels), visits);
    }

    [Fact]
    public async Task Handle_AddsVisit_OnValidInput()
    {
        var (handler, repo) = Build();

        var result = await handler.Handle(
            new CreateVisitationCommand(1, 1, new DateTime(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Single(repo.Store);
    }

    [Fact]
    public async Task Handle_Throws_WhenCustomerMissing()
    {
        var (handler, _) = Build();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateVisitationCommand(999, 1, new DateTime(2024, 5, 1)), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenHotelMissing()
    {
        var (handler, _) = Build();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateVisitationCommand(1, 999, new DateTime(2024, 5, 1)), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenVisitDateMissing()
    {
        var (handler, _) = Build();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateVisitationCommand(1, 1, default), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Throws_WhenIdsNonPositive()
    {
        var (handler, _) = Build();

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreateVisitationCommand(0, 0, new DateTime(2024, 5, 1)), CancellationToken.None));
    }
}
