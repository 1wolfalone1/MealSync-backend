using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MealSync.Application.Common.Enums;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
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
            _logger.LogError(exception, exception.Message);
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
        await HandleValidateClientExceptionAsync(context, HttpStatusCode.BadRequest, new ExceptionResponse(exception));
    }

    private async Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
    {
        await HandleClientExceptionAsync(context, HttpStatusCode.BadRequest, Array.Empty<object>(), new ExceptionResponse(exception));
    }

    private async Task HandleInValidBusinessExceptionAsync(HttpContext context, InvalidBusinessException exception)
    {
        await HandleClientExceptionAsync(context, exception.HttpStatusCode, exception.Args, new ExceptionResponse(exception));
    }

    private async Task HandleClientExceptionAsync(HttpContext context, HttpStatusCode code, object[] args,
        ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        var error = args.Length > 0
            ? new Error(
                response.Message, args)
            : new Error(
                response.Message);

        var options = new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(error), options));
    }

    private async Task HandleValidateClientExceptionAsync(HttpContext context, HttpStatusCode code,
        ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var options = new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure<Dictionary<string, List<string>>>
            (response.Details, new Error(ResponseCode.ValidationError.GetDescription(), string.Empty)), options));
    }

    private async Task HandleSystemExceptionAsync(HttpContext context, HttpStatusCode code, ExceptionResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var options = new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(Result.Failure(new Error(
                response.ErrorCode.ToString(),
                response.Details != null ? JsonConvert.SerializeObject(response.Details) : response.Message, true)),
            options));
    }
}