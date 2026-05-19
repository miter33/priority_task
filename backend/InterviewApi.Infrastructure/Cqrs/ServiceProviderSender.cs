using InterviewApi.Application.Common.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewApi.Infrastructure.Cqrs;

public class ServiceProviderSender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderSender(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
        Task<TResponse> task;
        try
        {
            task = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
        }
        catch (System.Reflection.TargetInvocationException tie) when (tie.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            throw;
        }
        return await task;
    }
}
