using System.Net;
using System.Text.Json;
using Homework.Api.Models;

namespace Homework.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        const HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

        var result = JsonSerializer.Serialize(new ErrorResponse((int)code, context.TraceIdentifier, ex.Message));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}