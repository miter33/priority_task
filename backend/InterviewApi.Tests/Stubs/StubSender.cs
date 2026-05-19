using InterviewApi.Application.Common.Cqrs;

namespace InterviewApi.Tests.Stubs;

internal class StubSender : ISender
{
    private readonly Dictionary<Type, Func<object, object>> _handlers = new();
    public List<object> SentRequests { get; } = new();

    public StubSender Handle<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        _handlers[typeof(TRequest)] = req => handler((TRequest)req)!;
        return this;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        SentRequests.Add(request);
        if (_handlers.TryGetValue(request.GetType(), out var fn))
        {
            return Task.FromResult((TResponse)fn(request));
        }
        throw new InvalidOperationException($"No stub handler registered for {request.GetType().Name}");
    }
}
