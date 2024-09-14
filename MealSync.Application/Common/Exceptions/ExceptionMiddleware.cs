using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MealSync.Domain.Exceptions.Base;
using MealSync.Domain.Shared;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MealSync.Application.Common.Exceptions;

public class ExceptionMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (ApiException exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleApiExceptionAsync(context, exception);
        }
        catch (ValidationException exception)
        {
            await HandleValidationExceptionASync(context, exception);
        }
        catch (InvalidBusinessException exception)
        {
            await HandleInValidBusinessExceptionAsync(context, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleInternalServerExceptionAsync(context, exception);
        }
    }

    private async Task HandleInternalServerExceptionAsync(HttpContext context, Exception exception)
    {
        await HandleSystemExceptionAsync(context, HttpStatusCode.InternalServerError, new ExceptionResponse(exception));
    }

    private async Task HandleValidationExceptionASync(HttpContext context, ValidationException exception)
    {
        await HandleClientExceptionAsync(context, HttpStatusCode.BadRequest, new ExceptionResponse(exception));
    }

    private async Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
    {
        await HandleClientExceptionAsync(context, HttpStatusCode.BadRequest, new ExceptionResponse(exception));
    }

    private async Task HandleInValidBusinessExceptionAsync(HttpContext context, InvalidBusinessException exception)
    {
        await HandleClientExceptionAsync(context, HttpStatusCode.BadRequest, new ExceptionResponse(exception));
    }

    private async Task HandleClientExceptionAsync(HttpContext context, HttpStatusCode code, ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true};  
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(new Error(response.ErrorCode.ToString(),
            response.Details != null ? JsonConvert.SerializeObject( response.Details ) : response.Message)), options));
    }

    private async Task HandleSystemExceptionAsync(HttpContext context, HttpStatusCode code, ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true};  
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(new Error(response.ErrorCode.ToString(),
            response.Details != null ? JsonConvert.SerializeObject( response.Details ) : response.Message, true)), options));
    }
}
