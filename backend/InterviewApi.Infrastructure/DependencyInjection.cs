using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Interfaces;
using InterviewApi.Infrastructure.Cqrs;
using InterviewApi.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICustomerRepository, JsonCustomerRepository>();
        services.AddSingleton<IHotelRepository, JsonHotelRepository>();
        services.AddSingleton<IVisitationRepository, JsonVisitationRepository>();

        services.AddScoped<ISender, ServiceProviderSender>();

        return services;
    }
}
