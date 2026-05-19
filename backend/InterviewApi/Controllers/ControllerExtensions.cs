using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Controllers;

internal static class ControllerExtensions
{
    public static ProblemDetails BodyMissing(string detail) => new()
    {
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
        Title = "One or more validation errors occurred.",
        Status = StatusCodes.Status400BadRequest,
        Detail = detail
    };

    public static ProblemDetails ParameterOutOfRange(string detail) => BodyMissing(detail);
}
