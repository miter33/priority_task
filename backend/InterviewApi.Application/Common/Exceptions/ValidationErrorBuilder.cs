namespace InterviewApi.Application.Common.Exceptions;

public class ValidationErrorBuilder
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public ValidationErrorBuilder Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var bucket))
        {
            _errors[field] = bucket = new List<string>();
        }
        bucket.Add(message);
        return this;
    }

    public bool HasErrors => _errors.Count > 0;

    public void ThrowIfAny()
    {
        if (!HasErrors) return;
        var snapshot = _errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
        throw new ValidationException(snapshot);
    }
}
