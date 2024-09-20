using System.Security.Claims;
using System.Text.Json;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.API.Middleware;

public class ActivityLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ActivityLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IActivityLogService activityLogService)
    {
        context.Request.EnableBuffering();

        // Log the incoming request
        await LogRequestAsync(context);

        // Call the next middleware in the pipeline
        await _next(context);

        // Log the response request
        await LogResponseAsync(context, activityLogService);
    }

    private async Task LogRequestAsync(HttpContext context)
    {
    }

    private async Task LogResponseAsync(HttpContext context, IActivityLogService activityLogService)
    {
        // Activity log of moderator
        var identity = context?.User.Identity as ClaimsIdentity;

        if (identity == null || !identity.IsAuthenticated)
            return;

        var claims = identity.Claims;

        var role = claims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value ?? null;

        if (role == default || role != Roles.Moderator.GetDescription())
            return;

        var httpMethod = context.Request.Method;
        var actionType = EnumHelper.GetEnumByDescription<ModeratorActionTypes>(httpMethod);

        if (actionType != default && actionType != ModeratorActionTypes.Read)
        {
            var targetEntity = (context.Request.Path.ToUriComponent().Split("/")[4]);
            var targetType = EnumHelper.GetEnumByDescription<ModeratorTargetTypes>(targetEntity);

            var response = await ReadRequestBodyAsync(context);
            var id = ExtractIdFromResponse(response);
            var accountId = claims.FirstOrDefault(o => o.Type == ClaimTypes.Sid).Value;

            var log = new ActivityLog()
            {
                AccountId = Int64.Parse(accountId),
                ActionType = actionType,
                TargetType = targetType,
                TargetId = id,
                ActionDetail = $"Requested {context.Request.Path} with body {response}",
                IsSuccess = context.Response.StatusCode == StatusCodes.Status200OK,
            };
            await activityLogService.LogActivity(log);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body);
        return await reader.ReadToEndAsync();
    }

    private long? ExtractIdFromResponse(string response)
    {
        using var jsonDocument = JsonDocument.Parse(response);
        if (jsonDocument.RootElement.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.Number)
        {
            return idElement.GetInt64();
        }
        return null;
    }
}