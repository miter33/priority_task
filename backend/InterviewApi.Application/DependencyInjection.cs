using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Customers.Commands.CreateCustomer;
using InterviewApi.Application.Customers.Queries.GetCustomerById;
using InterviewApi.Application.Customers.Queries.ListCustomers;
using InterviewApi.Application.Hotels.Queries.GetHotelById;
using InterviewApi.Application.Hotels.Queries.ListHotels;
using InterviewApi.Application.Visitations.Commands.CreateVisitation;
using InterviewApi.Application.Visitations.Queries.SearchVisitations;
using InterviewApi.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<GetCustomerByIdQuery, Customer>, GetCustomerByIdQueryHandler>();
        services.AddScoped<IRequestHandler<ListCustomersQuery, IReadOnlyList<Customer>>, ListCustomersQueryHandler>();
        services.AddScoped<IRequestHandler<CreateCustomerCommand, Customer>, CreateCustomerCommandHandler>();

        services.AddScoped<IRequestHandler<ListHotelsQuery, IReadOnlyList<Hotel>>, ListHotelsQueryHandler>();
        services.AddScoped<IRequestHandler<GetHotelByIdQuery, Hotel>, GetHotelByIdQueryHandler>();

        services.AddScoped<IRequestHandler<SearchVisitationsQuery, IReadOnlyList<Common.Dtos.VisitationView>>, SearchVisitationsQueryHandler>();
        services.AddScoped<IRequestHandler<CreateVisitationCommand, Visitation>, CreateVisitationCommandHandler>();

        return services;
    }
}
