using InterviewApi.Domain.Entities;
using InterviewApi.Infrastructure.Persistence;
using Xunit;

namespace InterviewApi.Tests.Infrastructure.Persistence;

public class JsonVisitationRepositoryTests
{
    [Fact]
    public void GetAll_LoadsFromJsonFile()
    {
        var repo = new JsonVisitationRepository(TestHelpers.MakeEnv());

        Assert.Equal(7, repo.GetAll().Count);
    }

    [Fact]
    public void Add_AssignsNextId_AndAppendsToCollection()
    {
        var repo = new JsonVisitationRepository(TestHelpers.MakeEnv());

        var v = repo.Add(new Visitation { CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 3, 3) });

        Assert.Equal(8, v.Id);
        Assert.Equal(8, repo.GetAll().Count);
    }

    [Fact]
    public void Add_StartsAtIdOne_WhenSourceFileEmpty()
    {
        var repo = new JsonVisitationRepository(
            TestHelpers.MakeEnv(visitationsJson: """{ "visitations": [] }"""));

        var v = repo.Add(new Visitation { CustomerId = 1, HotelId = 1, VisitDate = new DateTime(2024, 1, 1) });

        Assert.Equal(1, v.Id);
    }
}
