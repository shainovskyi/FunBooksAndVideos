using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var pd = GetProblemDetails(ex, context);

            context.Response.StatusCode = pd.Status ?? StatusCodes.Status500InternalServerError;

            logger.LogError(ex, "Unhandled exception (StatusCode: {StatusCode}) while processing {Path}", context.Response.StatusCode, context.Request.Path);

            await context.Response.WriteAsJsonAsync(pd);
        }
    }

    private static ProblemDetails GetProblemDetails(Exception ex, HttpContext context)
    {
        var (statusCode, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
            DomainException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError ? null : ex.Message,
            Instance = context.Request.Path
        };
    }
}
