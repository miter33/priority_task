using System.Text.Json;
using InterviewApi.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            var pd = new ValidationProblemDetails(vex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };
            await WriteAsync(context, pd);
        }
        catch (NotFoundException nex)
        {
            var pd = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                Title = "Resource not found.",
                Status = StatusCodes.Status404NotFound,
                Detail = nex.Message,
                Instance = context.Request.Path
            };
            await WriteAsync(context, pd);
        }
        catch (ConflictException cex)
        {
            var pd = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                Title = "Conflict with the current state of the resource.",
                Status = StatusCodes.Status409Conflict,
                Detail = cex.Message,
                Instance = context.Request.Path
            };
            await WriteAsync(context, pd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var pd = new ProblemDetails
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Unexpected server error.",
                Instance = context.Request.Path
            };
            await WriteAsync(context, pd);
        }
    }

    private static Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, problem.GetType(), JsonOptions));
    }
}
